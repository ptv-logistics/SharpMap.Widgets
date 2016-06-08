using GeoAPI.Geometries;
using System;

namespace Tools
{
    public static class GeoTools
    {
        public static Coordinate Wgs2SphereMercator(Coordinate point)
        {
            return new Coordinate(6371000.0 * point.X * Math.PI / 180.0,
                6371000.0 * Math.Log(Math.Tan(Math.PI / 4.0 + point.Y * Math.PI / 360.0)));
        }

        public static Coordinate SphereMercator2Wgs(Coordinate point)
        {
            return new Coordinate((180.0 / Math.PI) * (point.X / 6371000.0),
                (360 / Math.PI) * (Math.Atan(Math.Exp(point.Y / 6371000.0)) - (Math.PI / 4)));
        }

        public static Envelope Wgs2SphereMercator(Envelope envelope)
        {
            return new Envelope(Wgs2SphereMercator(envelope.TopLeft()), Wgs2SphereMercator(envelope.BottomRight()));
        }

        public static Envelope SphereMercator2Wgs(Envelope envelope)
        {
            return new Envelope(SphereMercator2Wgs(envelope.TopLeft()), SphereMercator2Wgs(envelope.BottomRight()));
        }
    }
}
