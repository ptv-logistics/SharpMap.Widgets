using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Data;
using SharpMap.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Widgets;

namespace SharpMap.Common
{
    public class HitTester
    {
        public static FeatureDataRow HitTest(IEnumerable<LayerInfo> layers, double lat, double lon, double z)
        {
            // we assume the half size for symbols is 8 pixel
            var size2 = 8;

            // calculate mercator units per pixel
            var mercRadius = 6371000.0 * 2 * Math.PI;
            var pixelsPerLevel = 256 * Math.Pow(2, z);
            var mercPerPix = mercRadius / pixelsPerLevel;

            // now calculate a mercator envelope of the corresponsing size
            var mSize = mercPerPix * size2;
            var mp = GeoTools.Wgs2SphereMercator(new Coordinate(lon, lat), true);
            var envelope = new Envelope(mp.X - mSize, mp.X + mSize, mp.Y - mSize, mp.Y + mSize);

            // and transform it to wgs
            var wgsEnvelope = GeoTools.SphereMercator2Wgs(envelope, true);

            // get all fg (= point) vector layers
            var fgTopDown = (from l in layers.GetLayers(RenderingLayer.Foreground) where l is VectorLayer select l as VectorLayer).Reverse();

            FeatureDataRow row = null;
            foreach (var l in fgTopDown)
            {
                // hit-test on symbols with the symbol-size envelope
                row = HitTest(l, wgsEnvelope);

                // symbol found -> return
                if (row != null)
                {
                    return row;
                }
            }

            // get all bg (= area) vector layers
            var bgTopDown = (from l in layers.GetLayers(RenderingLayer.Background) where l is VectorLayer select l as VectorLayer).Reverse();

            foreach (var l in bgTopDown)
            {
                // hit-test on areas only with the point
                row = HitTest(l, new Envelope(lon, lon, lat, lat));

                // area found -> return
                if (row != null)
                    return row;
            }

            // noting found
            return null;
        }

        public static FeatureDataRow HitTest(VectorLayer l, Envelope wgsEnvelope)
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
                if (x.Geometry.Intersects(geom))
                    return x;
            }

            // nothing was hit
            return null;

        }

        public static object HitTest(object sampleInfo, double y, double x, double currentZoom)
        {
            throw new NotImplementedException();
        }
    }
}