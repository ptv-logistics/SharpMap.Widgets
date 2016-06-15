using GeoAPI.Geometries;
using System;

namespace Tools
{
    public static class GeoTools
    {
        public static Coordinate Wgs2SphereMercator(this Coordinate point, bool usePtvRadius = false)
        {
            var radius = usePtvRadius ? 6371000.0 : 6378137.0;

            return new Coordinate(radius * point.X * Math.PI / 180.0,
                radius * Math.Log(Math.Tan(Math.PI / 4.0 + point.Y * Math.PI / 360.0)));
        }

        public static Coordinate SphereMercator2Wgs(this Coordinate point, bool usePtvRadius = false)
        {
            var radius = usePtvRadius ? 6371000.0 : 6378137.0;

            return new Coordinate((180.0 / Math.PI) * (point.X / radius),
                (360 / Math.PI) * (Math.Atan(Math.Exp(point.Y / radius)) - (Math.PI / 4)));
        }

        public static Envelope Wgs2SphereMercator(this Envelope envelope, bool usePtvRadius = false)
        {
            return new Envelope(Wgs2SphereMercator(envelope.TopLeft(), usePtvRadius), 
                Wgs2SphereMercator(envelope.BottomRight(), usePtvRadius));
        }

        public static Envelope SphereMercator2Wgs(this Envelope envelope, bool usePtvRadius = false)
        {
            return new Envelope(SphereMercator2Wgs(envelope.TopLeft(), usePtvRadius),
                SphereMercator2Wgs(envelope.BottomRight(), usePtvRadius));
        }
    }
}
