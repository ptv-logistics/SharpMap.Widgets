using Widgets;
using GeoAPI.Geometries;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Web;

namespace SpatialTutorial
{
    /// <summary>
    /// Summary description for DynamicTilesHandler
    /// </summary>
    public class SharpMapOverlayHandler : IHttpHandler
    {
        // http://msdn.microsoft.com/en-us/library/bb259689.aspx
        public void ProcessRequest(HttpContext context)
        {
            int width, height;

            var c = new Func<string, double>(s => System.Convert.ToDouble(s, CultureInfo.InvariantCulture));

            //Parse request parameters
            var bbox = context.Request.Params["bbox"].Split(',');
            var envelope = new Envelope(c(bbox[0]), c(bbox[2]), c(bbox[1]), c(bbox[3]));
            if (!int.TryParse(context.Request.Params["width"], out width))
                throw (new ArgumentException("Invalid parameter"));
            if (!int.TryParse(context.Request.Params["height"], out height))
                throw (new ArgumentException("Invalid parameter"));

            // create a transparent sharpmap map with a size of 256x256
            using (var sharpMap = new SharpMap.Map(new Size(width, height)) { BackColor = Color.Transparent })
            {
                // add the layer to the map
                foreach (var l in LayerFactories.FgFactory(sharpMap))
                    sharpMap.Layers.Add(l);

                // calculate the bbox for the tile key and zoom the map 
                sharpMap.ZoomToBox(envelope);

                // render the map image
                using (var img = sharpMap.GetMap())
                {
                    // stream the image to the client
                    var memoryStream = new MemoryStream();

                    // Saving a PNG image requires a seekable stream, first save to memory stream 
                    // http://forums.asp.net/p/975883/3646110.aspx#1291641
                    img.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var buffer = memoryStream.ToArray();

                    context.Response.ContentType = "image/png";
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        public const double EarthRadius = 6378137.0;

        /// <summary> Calculates a Mercator bounding box for a tile key. </summary>
        /// <param name="tileX"> The tile x coordinate in PTV-internal format. </param>
        /// <param name="tileY"> The tile y coordinate in PTV-internal format. </param>
        /// <param name="zoom"> The zoom level. </param>
        /// <returns> A bounding box in Mercator format which corresponds to the given tile coordinates and zoom level. </returns>
        public static Envelope TileToMercatorAtZoom(int tileX, int tileY, int zoom)
        {
            const double earthCircum = EarthRadius * 2.0 * Math.PI;
            const double earthHalfCircum = earthCircum / 2;
            double arc = earthCircum / (1 << zoom);

            return new Envelope(
                (tileX * arc) - earthHalfCircum, ((tileX + 1) * arc) - earthHalfCircum,
                earthHalfCircum - ((tileY + 1) * arc), earthHalfCircum - (tileY * arc));
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}