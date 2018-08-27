using GeoAPI.Geometries;
using SharpMap.Common;
using System;
using System.Drawing;
using BruTile;
using BruTile.PreDefined;
using BruTile.Web;
using Tools;
using BruTile.Cache;
using System.Collections.Generic;

namespace SharpMap.Print
{
    class Program
    {
        static void Main(string[] args)
        {
            // this is a time-limited demo token. 
            // You need your own xServer-internet token for your application!
            var token = "9358789A-A8CF-4CA8-AC99-1C0C4AC07F1E";

            // bitmap size
            int width = 950;
            int height = 600;
            
            // get unique file name
            var fileName = System.IO.Path.GetTempPath() + "\\Img" + Guid.NewGuid() + ".jpg";
            
            // lat/lng bounds
            var envelope = new Envelope(8.4, 8.5, 49, 49.1); // EU
            // var envelope = new Envelope(6, 11, 47, 52); // KA

            // create our sample data layers
            var sampleInfo = Widgets.SampleLayers.Layers;

            // create a transparent sharpmap map 
            using (var sharpMap = new Map(new Size(width, height)) { BackColor = Color.Transparent })
            {
                // set map viewport (in mercator)
                sharpMap.ZoomToBox(envelope.Wgs2SphereMercator());

                // HERE satellite images
                //sharpMap.Layers.Add(new Layers.TileLayer(new TileSource(new WebTileProvider(new HereSatelliteRequest()
                //{ AppId = "<your app id>", AppCode = "<your app code>" }, new NullCache()), // default cache doesn't work!?
                //new SphericalMercatorInvertedWorldSchema()), "HERE"));

                // map2 road overlay
                //sharpMap.Layers.Add(new Layers.TileLayer(new TileSource(new WebTileProvider(new XMapTwoRequests()
                //{ XTok = token, Profile = "silkysand-background" }, new NullCache()), // default cache doesn't work!?
                //new SphericalMercatorInvertedWorldSchema()), "XMAP2"));

                // test for displaying traffic incidents
                var customXMapLayers = new List<XMapServiceReference.Layer>
                {
//                    new XMapServiceReference.FeatureLayer {name = "PTV_TrafficIncidents", visible = true}
                };

                // xmap-bg
                sharpMap.Layers.Add(new XMapLayer("xmap-bg", "https://api-eu-test.cloud.ptvgroup.com/xmap/ws/XMap", MapMode.Background)
                { User = "xtok", Password = token, CustomXMapLayers = customXMapLayers /* Opacity = 0.5F */ }); // set semi-opaque for hybrid view

                // areas
                foreach (var l in sampleInfo.GetLayers(RenderingLayer.Background, sharpMap.PixelSize))
                    sharpMap.Layers.Add(l);

                // xmap-fg
                sharpMap.Layers.Add(new XMapLayer("xmap-fg", "https://api-eu-test.cloud.ptvgroup.com/xmap/ws/XMap", MapMode.Town)
                { User = "xtok", Password = token, CustomXMapLayers = customXMapLayers });

                // POIs
                foreach (var l in sampleInfo.GetLayers(RenderingLayer.Foreground, sharpMap.PixelSize))
                    sharpMap.Layers.Add(l);

                using (var image = sharpMap.GetMap())
                using (var graphics = Graphics.FromImage(image))
                {
                    // put a custom title on the image
                    using (var font = new System.Drawing.Font("Arial", 16))
                    {
                        var text = "Hello!\nThis is a custom title.";
                        var textSize = graphics.MeasureString(text, font);

                        // draw a box @top/center with border 4 px
                        int borderSize = 4;
                        graphics.FillRectangle(Brushes.White, width / 2 - textSize.Width / 2 - borderSize, 0, textSize.Width + 8, textSize.Height + 8);
                        graphics.DrawRectangle(Pens.Black, width / 2 - textSize.Width / 2 - 4, 0, textSize.Width + 8, textSize.Height + 8);

                        // draw text
                        graphics.DrawString(text, font, Brushes.Black, new PointF(width / 2 - textSize.Width / 2, 4));
                    }

                    // save image to disk
                    image.Save(fileName);
                }

                // open the image
                System.Diagnostics.Process.Start(fileName);
            }
        }
    }
}