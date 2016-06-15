using System;
using System.Collections.Generic;
using System.Text;
using SharpMap.Layers;
using System.Net;
using System.IO;
using System.Net.Cache;
using GeoAPI.Geometries;
using SharpMap.Print.XMapServiceReference;

namespace SharpMap.Print
{
    /// <summary>
    /// XmapRenderer renders an xMapServer-Bitmap 
    /// This implementation fixes several problems which occur in combination with tiling and silverlight
    /// + it clips the request rectangles to avoid problems on the southern hemisphere
    /// + it internally resizes the image to avoid artifacts on tile borders
    /// + it makes the labels transparent
    /// + it converts gif to png
    /// </summary>
    public class XMapRenderer
    {
        int minX = -20000000;
        int maxX = 20000000;
        int minY = -10000000;
        int maxY = 20000000;

        private string url = string.Empty;
        private MapMode mapMode;

        public string User { get; set; }

        public string Password { get; set; }

        public XMapRenderer(string url, MapMode mapMode)
        {
            // test of clipping
            //minX = 937117 -1000000;
            //maxX = 937117 + 1000000;
            //minY = 6270145 -1000000;
            //maxY = 6270145 + 1000000;

            this.mapMode = mapMode;
            this.url = url;
        }

        public Stream GetStream(int left, int top, int right, int bottom, int width, int height, ImageFileFormat format)
        {
            int trials = 0;

            while (true)
            {
                try
                {
                    return TryGetStream(left, top, right, bottom, width, height, format);
                }
                catch (WebException exception)
                {
                    // retry for 500 and 503
                    var result = (HttpWebResponse)exception.Response;
                    if (result.StatusCode == HttpStatusCode.InternalServerError ||
                        result.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        if (++trials < 3)
                        {
                            System.Threading.Thread.Sleep(50);
                            continue;
                        }
                    }

                    throw;
                }
            }
        }

        public Stream TryGetStream(int left, int top, int right, int bottom, int width, int height, ImageFileFormat format)
        {
            var boundingBox = new BoundingBox
            {
                leftTop = new Point { point = new PlainPoint { x = left, y = top } },
                rightBottom = new Point { point = new PlainPoint { x = right, y = bottom } }
            };

            var mapParams = new MapParams
            {
                showScale = false,
                useMiles = false
            };

            var imageInfo = new ImageInfo { format = format, height = height, width = width };

            string profile = string.Empty;
            var layers = new List<XMapServiceReference.Layer>();
            switch (mapMode)
            {
                // only streets
                case MapMode.Street:
                    {
                        layers.Add(new StaticPoiLayer { name = "town", visible = false, category = -1, detailLevel = 0 });
                        layers.Add(new StaticPoiLayer { name = "background", visible = false, category = -1, detailLevel = 0 });

                        profile = "ajax-bg";

                        break;
                    }
                // only labels
                case MapMode.Town:
                    {
                        profile = "ajax-fg";

                        break;
                    }
                // nothing
                case MapMode.Custom:
                    {
                        profile = "ajax-fg";
                        layers.Add(new StaticPoiLayer { name = "town", visible = false, category = -1, detailLevel = 0 });
                        layers.Add(new StaticPoiLayer { name = "street", visible = false, category = -1, detailLevel = 0 });
                        layers.Add(new StaticPoiLayer { name = "background", visible = false, category = -1, detailLevel = 0 });

                        break;
                    }
                // only background
                case MapMode.Background:
                    {
                        layers.Add(new StaticPoiLayer { name = "town", visible = false, category = -1, detailLevel = 0 });
                        profile = "ajax-bg";

                        break;
                    }
            }

            //// add custom xmap layrs
            //if (CustomXmapLayers != null)
            //    layers.AddRange(CustomXmapLayers);

            var callerContext = new CallerContext
            {
                wrappedProperties = new CallerContextProperty[]{
                        new CallerContextProperty{key = "CoordFormat", value="PTV_MERCATOR"},
                        new CallerContextProperty{key = "Profile", value = profile}
                    }
            };

            var client = new XMapWSClient("XMapWSPort", this.url);

            if(!string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password))
            {
                client.ClientCredentials.UserName.UserName = User;
                client.ClientCredentials.UserName.Password = Password;
            }

            var map = client.renderMapBoundingBox(boundingBox, mapParams, imageInfo, layers.ToArray(), true, callerContext);

            return new MemoryStream(map.image.rawImage);
        }

        private System.Drawing.Bitmap SaveAndConvert(System.Drawing.Bitmap image)
        {
            // make background transparent for overlays
            if (mapMode != MapMode.Background)
                image.MakeTransparent(System.Drawing.Color.FromArgb(255, 254, 185));

            return image;
        }

        public System.Drawing.Bitmap GetImage(int left, int top, int right, int bottom, int width, int height, int border)
        {
            if (top > bottom)
            {
                var t = top;
                top = bottom;
                bottom = t;
            }

            if (left < minX || right > maxX || top < minY || bottom > maxY || border > 0)
            {
                // request must be resized or clipped
                double leftResized, rightResized, topResized, bottomResized;

                // calculate resized bounds depending on border
                // the resize factor internally resizes requested tiles to avoid clipping problems
                if (border > 0)
                {
                    double resize = (double)border / width;
                    double lWidth = (right - left) * resize;
                    double lHeight = (bottom - top) * resize;

                    leftResized = (left - lWidth);
                    rightResized = (right + lWidth);
                    topResized = (top - lHeight);
                    bottomResized = (bottom + lHeight);
                }
                else
                {
                    leftResized = left;
                    rightResized = right;
                    topResized = top;
                    bottomResized = bottom;
                }

                // calculate clipped bounds
                double leftClipped = (leftResized < minX) ? minX : leftResized;
                double rightClipped = (rightResized > maxX) ? maxX : rightResized;
                double topClipped = (topResized < minY) ? minY : topResized;
                double bottomClipped = (bottomResized > maxY) ? maxY : bottomResized;

                // calculate corresponding pixel width and height 
                double rWidth = width * (rightClipped - leftClipped) / (right - left);
                double rHeight = height * (bottomClipped - topClipped) / (bottom - top);

                if (rWidth < 32 || rHeight < 32)
                {
                    // resulting image will be too small -> return empty image
                    var bmp = new System.Drawing.Bitmap(width, height);
                    return SaveAndConvert(bmp);
                }
                else using (System.IO.Stream stream = GetStream(
                    (int)Math.Round(leftClipped), (int)Math.Round(topClipped), (int)Math.Round(rightClipped), (int)Math.Round(bottomClipped),
                    (int)Math.Round(rWidth), (int)Math.Round(rHeight), ImageFileFormat.GIF))
                    {
                        // paste resized/clipped image on new image
                        using (var img = System.Drawing.Image.FromStream(stream))
                        {
                            var bmp = new System.Drawing.Bitmap(width, height);

                            using (var g = System.Drawing.Graphics.FromImage(bmp))
                            {
                                double offsetX = (leftClipped - left) / (right - left) * width;
                                double offsetY = (bottomClipped - bottom) / (top - bottom) * height;

                                g.DrawImageUnscaled(img, (int)Math.Round(offsetX), (int)Math.Round(offsetY));
                            }

                            return SaveAndConvert(bmp);
                        }
                    }
            }        
            else using (var stream = GetStream(left, top, right, bottom, width, height, ImageFileFormat.GIF))
                {
                    var img = System.Drawing.Image.FromStream(stream) as System.Drawing.Bitmap;
                        return SaveAndConvert(img);
                }
        }
    }

    public enum MapMode
    {
        Background,
        Town,
        Street,
        Custom
    }
}