using Widgets;
using GeoAPI.Geometries;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

namespace SpatialTutorial
{
    /// <summary>
    /// Summary description for DynamicTilesHandler
    /// </summary>
    public class SharpMapTilesHandler : IHttpHandler
    {
        // http://msdn.microsoft.com/en-us/library/bb259689.aspx
        public void ProcessRequest(HttpContext context)
        {
            int x, y, z;

            //Parse request parameters
            if (!int.TryParse(context.Request.Params["x"], out x))
                throw (new ArgumentException("Invalid parameter"));
            if (!int.TryParse(context.Request.Params["y"], out y))
                throw (new ArgumentException("Invalid parameter"));
            if (!int.TryParse(context.Request.Params["z"], out z))
                throw (new ArgumentException("Invalid parameter"));

            // create a transparent sharpmap map with a size of 256x256
            using (var sharpMap = new SharpMap.Map(new Size(256, 256)) { BackColor = Color.Transparent })
            {
                // add the layer to the map
                foreach (var l in LayerFactories.BgFactory(sharpMap))
                    sharpMap.Layers.Add(l);

                // calculate the bbox for the tile key and zoom the map 
                sharpMap.ZoomToBox(TileToMercatorAtZoom(x,y,z));

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