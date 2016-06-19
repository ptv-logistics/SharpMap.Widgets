using GeoAPI.Geometries;
using SharpMap.Common;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Web;
using Widgets;
using System.Linq;

namespace SpatialTutorial
{
    /// <summary>
    /// Summary description for DynamicTilesHandler
    /// </summary>
    public class SharpMapOverlayHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            int width, height;

            var c = new Func<string, double>(s => System.Convert.ToDouble(s, CultureInfo.InvariantCulture));

            //Parse request parameters
            var bbox = context.Request.Params["bbox"].Split(',');
            var envelope = new Envelope(c(bbox[0]), c(bbox[2]), c(bbox[1]), c(bbox[3]));
            var layers = context.Request.Params["layers"].Split(',');
            if (!int.TryParse(context.Request.Params["width"], out width))
                throw (new ArgumentException("Invalid parameter"));
            if (!int.TryParse(context.Request.Params["height"], out height))
                throw (new ArgumentException("Invalid parameter"));

            // create a transparent sharpmap map with a size of the requested size
            using (var sharpMap = new SharpMap.Map(new Size(width, height)) { BackColor = Color.Transparent })
            {
                // add the layer to the map
                var bgLayers = from l in SampleLayers.Layers.GetLayers(RenderingLayer.Foreground, sharpMap.PixelSize) select l;
                var requestLayers = (from l in bgLayers where layers.Contains(l.LayerName) select l).ToList();

                if (requestLayers.Count == 0)
                    return;

                foreach (var l in requestLayers)
                    sharpMap.Layers.Add(l);
                
                // zoom to the requested envelope 
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

        public bool IsReusable
        {
            get { return true; }
        }
    }
}