using GeoAPI.Geometries;
using SharpMap.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace SharpMap.Print
{
    class Program
    {
        static void Main(string[] args)
        {
            // this is a time-limited demo token. 
            // You need your own xServer-internet token for your application!
            var token = "953B0471-1EB8-4E1C-B170-ACDF1B04D6B5";

            int width = 950;
            int height = 600;
            // get unique file name
            var fileName = System.IO.Path.GetTempPath() + "\\Img" + Guid.NewGuid().ToString() + ".jpg";
            // lat/lng bounds
            var envelope = new Envelope(6, 11, 47, 52);

            var sampleInfo = Widgets.SampleLayers.Layers;

            // create a transparent sharpmap map with a size of 256x256
            using (var sharpMap = new SharpMap.Map(new Size(width, height)) { BackColor = Color.Transparent })
            {
                // calculate the bbox for the tile key and zoom the map 
                sharpMap.ZoomToBox(GeoTools.Wgs2SphereMercator(envelope));

                // xmap-bg
                sharpMap.Layers.Add(new XMapLayer("xmap-bg", "https://api-eu-test.cloud.ptvgroup.com/xmap/ws/XMap", MapMode.Background)
                { User = "xtok", Password = token });

                // areas
                foreach (var l in sampleInfo.GetLayers(RenderingLayer.Background, sharpMap.PixelSize))
                    sharpMap.Layers.Add(l);

                // xmap-fg
                sharpMap.Layers.Add(new XMapLayer("xmap-fg", "https://api-eu-test.cloud.ptvgroup.com/xmap/ws/XMap", MapMode.Town)
                { User = "xtok", Password = token });

                // POIs
                foreach (var l in sampleInfo.GetLayers(RenderingLayer.Foreground, sharpMap.PixelSize))
                    sharpMap.Layers.Add(l);

                using (var image = sharpMap.GetMap())
                using (var graphics = System.Drawing.Graphics.FromImage(image))
                {
                    // put a custom title on the image
                    using (var font = new System.Drawing.Font("Arial", 16))
                    {
                        var text = "Hello!\nThis is a custom title.";
                        var textSize = graphics.MeasureString(text, font);

                        // draw a box @top/center with border 4 px
                        int borderSize = 4;
                        graphics.FillRectangle(System.Drawing.Brushes.White, width / 2 - textSize.Width / 2 - borderSize, 0, textSize.Width + 8, textSize.Height + 8);
                        graphics.DrawRectangle(System.Drawing.Pens.Black, width / 2 - textSize.Width / 2 - 4, 0, textSize.Width + 8, textSize.Height + 8);

                        // draw text
                        graphics.DrawString(text, font, System.Drawing.Brushes.Black, new System.Drawing.PointF(width / 2 - textSize.Width / 2, 4));
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