using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Common;
using SharpMap.Data;
using SharpMap.Layers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Tools;
using Widgets;

namespace SpatialTutorial
{
    /// <summary>
    /// Summary description for SpatialPickHandler
    /// </summary>
    public class SpatialPickingHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                //Parse request parameters
                double lat, lng, z;
                if (!double.TryParse(context.Request.Params["lat"], NumberStyles.Float, CultureInfo.InvariantCulture, out lat))
                    throw (new ArgumentException("Invalid parameter"));
                if (!double.TryParse(context.Request.Params["lng"], NumberStyles.Float, CultureInfo.InvariantCulture, out lng))
                    throw (new ArgumentException("Invalid parameter"));
                if (!double.TryParse(context.Request.Params["z"], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                    throw (new ArgumentException("Invalid parameter"));

                var layers = context.Request.Params["layers"].Split(',');

                context.Response.ContentType = "text/json";

                var requestLayers = (from l in SampleLayers.Layers where layers.Contains(l.Name) select l).ToList();

                var hitObject = HitTester.HitTest(requestLayers, lat, lng, z); 
                if(hitObject!=null)
                    WriteRow(hitObject, context.Response);
                else
                    context.Response.Write("{}");
            }
            catch (Exception ex)
            {
                // no result - return empty json
                context.Response.Write(@"{  ""error"": """ + ex.Message + @"""}");
            }
        }

        public void WriteRow(FeatureDataRow row, HttpResponse response)
        {
            // dump all attributes
            var d = new Dictionary<string, object>();
            foreach (DataColumn c in row.Table.Columns)
            {
                d[c.ColumnName] = row[c];
            }

            // create a geojson
            var gjs = new SharpMap.Converters.GeoJSON.GeoJSON(row.Geometry, d);

            // and write it to the response
            var sw = new StringWriter();
            SharpMap.Converters.GeoJSON.GeoJSONWriter.Write(gjs, sw);
            response.Write(sw);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}