using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using SharpMap.Layers;
using System.Net;
using System.IO;
using System.Net.Cache;
using GeoAPI.Geometries;
using SharpMap.Print.XMapServiceReference;
using Layer = SharpMap.Print.XMapServiceReference.Layer;
using Point = SharpMap.Print.XMapServiceReference.Point;

namespace SharpMap.Print
{

    /// <summary>
    /// XMapServerLayer renders a Bitmap using PTV xMapServer
    /// </summary>
    public class XMapLayer : SharpMap.Layers.Layer
    {
        public string User { get; set; }

        public string Password { get; set; }

        public float Opacity { get; set; }

        public XMapLayer(string name, string url, MapMode mapMode)
        {
            this.url = url;
            this.mapMode = mapMode;
            LayerName = name;
            SRID = 3857; // Web_MERCATOR
            Opacity = 1.0F;
        }

        public override Envelope Envelope
        {
            get
            {
                return null;
            }
        }

        public override void Render(System.Drawing.Graphics g, SharpMap.Map map)
        {
            base.Render(g, map);

            // For convenience and consistency 
            // we use SharpMap with Web- (aka Google-) Mercator. So just transform the envelope.
            var googleToPtv = 6371000.0 / 6378137.0;
            var envelope = new Envelope(map.Envelope.TopLeft().X * googleToPtv, map.Envelope.BottomRight().X * googleToPtv,
                map.Envelope.TopLeft().Y * googleToPtv, map.Envelope.BottomRight().Y * googleToPtv);

            DrawImage(g, (int)envelope.Left(), (int)envelope.Top(), (int)envelope.Right(), (int)envelope.Bottom(), 
                map.Size.Width, map.Size.Height, 0, Opacity);
        }

        int minX = -20000000;
        int maxX = 20000000;
        int minY = -10000000;
        int maxY = 20000000;

        private string url = string.Empty;
        private MapMode mapMode;

        /// <summary> Gets or sets the custom layers of the xMapServer. </summary>
        public IEnumerable<Layer> CustomXMapLayers { get; set; }

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

        public Stream TryGetStream(int left, int top, int right, int bottom, int width, int height,
            ImageFileFormat format)
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

            // add custom xmap layrs
            if (CustomXMapLayers != null)
                layers.AddRange(CustomXMapLayers);

            var callerContext = new CallerContext
            {
                wrappedProperties = new CallerContextProperty[]
                {
                    new CallerContextProperty {key = "CoordFormat", value = "PTV_MERCATOR"},
                    new CallerContextProperty {key = "Profile", value = profile}
                }
            };

            var client = new XMapWSClient("XMapWSPort", this.url);

            if (!string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password))
            {
                client.ClientCredentials.UserName.UserName = User;
                client.ClientCredentials.UserName.Password = Password;
            }

            var map = client.renderMapBoundingBox(boundingBox, mapParams, imageInfo, layers.ToArray(), true,
                callerContext);

            return new MemoryStream(map.image.rawImage);
        }

        public void DrawImage(Graphics g, int left, int top, int right, int bottom, int width, int height, int border,
            float opacity)
        {
            if (top > bottom)
            {
                var t = top;
                top = bottom;
                bottom = t;
            }

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
                // resulting image will be too small -> return
                return;
            }

            using (var stream = GetStream(
                (int)Math.Round(leftClipped), (int)Math.Round(topClipped), (int)Math.Round(rightClipped),
                (int)Math.Round(bottomClipped),
                (int)Math.Round(rWidth), (int)Math.Round(rHeight), ImageFileFormat.PNG))
            {
                // paste resized/clipped image on new graphics
                using (var img = System.Drawing.Image.FromStream(stream))
                {
                    double offsetX = (leftClipped - left) / (right - left) * width;
                    double offsetY = (bottomClipped - bottom) / (top - bottom) * height;

                    if (opacity != 1.0F)
                    {
                        //create a color matrix object  
                        ColorMatrix matrix = new ColorMatrix();

                        //set the opacity  
                        matrix.Matrix33 = opacity;

                        using (var attributes = new ImageAttributes())
                        {

                            //set the color(opacity) of the image  
                            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                            g.DrawImage(img,
                                new Rectangle((int)Math.Round(offsetX), (int)Math.Round(offsetY),
                                    img.Width,
                                    img.Height),
                                0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attributes);
                        }
                    }
                    else
                        g.DrawImageUnscaled(img, (int)Math.Round(offsetX), (int)Math.Round(offsetY));
                }
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
