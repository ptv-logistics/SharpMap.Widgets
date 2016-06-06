using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
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

                context.Response.ContentType = "text/json";

                // we assume the half size for symbols is 8 pixel
                var size2 = 8;

                // calculate mercator units per pixel
                var mercRadius = 6371000.0 * 2 * Math.PI;
                var pixelsPerLevel = 256 * Math.Pow(2, z);
                var mercPerPix = mercRadius / pixelsPerLevel;

                // now calculate a mercator envelope of the corresponsing size
                var mSize = mercPerPix * size2;
                var mp = GeoTools.Wgs2SphereMercator(new Coordinate(lng, lat));
                var envelope = new Envelope(mp.X - mSize, mp.X + mSize, mp.Y - mSize, mp.Y + mSize);

                // and transform it to wgs
                var wgsEnvelope = GeoTools.SphereMercator2Wgs(envelope);

                // get all fg (= point) vector layers
                var fgTopDown = (from l in LayerFactories.FgFactory() where l is VectorLayer select l as VectorLayer).Reverse();

                FeatureDataRow row = null;
                foreach (var l in fgTopDown)
                {
                    // hit-test on symbols with the symbol-size envelope
                    row = HitTest(l, wgsEnvelope);

                    // symbol found -> return
                    if (row != null)
                    {
                        WriteRow(row, context.Response);
                        return;
                    }
                }

                // get all bg (= area) vector layers
                var bgTopDown = (from l in LayerFactories.BgFactory() where l is VectorLayer select l as VectorLayer).Reverse();

                foreach (var l in bgTopDown)
                {
                    // hit-test on areas only with the point
                    row = HitTest(l, new Envelope(lng, lng, lat, lat));

                    // area found -> return
                    if (row != null)
                    {
                        WriteRow(row, context.Response);
                        return;
                    }
                }

                // noting found
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

        public FeatureDataRow HitTest(VectorLayer l, Envelope wgsEnvelope)
        {
            // search for candidates
            var ds = new FeatureDataSet();
            l.DataSource.Open();
            l.DataSource.ExecuteIntersectionQuery(wgsEnvelope, ds);
            l.DataSource.Close();
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            // if candidates are found, we have to check the exact intersection
            // create a geometry from the envelope
            IGeometry geom;
            if (wgsEnvelope.Area == 0)
                geom = new Point(wgsEnvelope.Centre);
            else
                geom = new Polygon(new LinearRing(new Coordinate[] {
                    wgsEnvelope.TopLeft(), wgsEnvelope.TopRight(),
                    wgsEnvelope.BottomRight(), wgsEnvelope.BottomLeft(), wgsEnvelope.TopLeft() }));

            // and return the topmost hit
            foreach (var x in (from FeatureDataRow row in ds.Tables[0].Rows select row).Reverse())
            {
                if(x.Geometry.Intersects(geom))
                    return x;
            }

            // nothing was hit
            return null;
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}