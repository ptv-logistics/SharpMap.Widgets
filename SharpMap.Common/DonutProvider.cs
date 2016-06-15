using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using Tools;

namespace Providers
{
    /// <summary>
    /// The donut provider returns an in-memory set of computed shapes as OGC geometries
    /// </summary>
    public class DonutProvider
    {
        // re-create our shapes
        public static IEnumerable<IGeometry> CreateRandomDonuts(int number)
        {
            // some reproduceable random sequence
            var rand = new Random(42);

            for (var dix = 0; dix < number; dix++)
            {
                // our donut parameters
                var lat = rand.NextDouble() * 4 + 45.0; // latitude
                var lon = rand.NextDouble() * 6 + 0.0; // logitude
                var rot = rand.NextDouble() * Math.PI; // rotation angle
                var radiusX = rand.NextDouble() * 20000.0 + 10000.0; // x-radius in m
                var radiusY = rand.NextDouble() * 20000.0 + 10000.0; // y-radius in m
                var buffer = 10000.0; // buffer size in m

                yield return CreateDonut(lat, lon, rot, radiusX, radiusY, buffer);
            }
        }

        // create a donut with the donut parameters
        public static IPolygon CreateDonut(
            double lat, double lon, double rot, double radiusX, double radiusY, double buffer)
        {
            // the donut shapes are calculated in a mercator (= conformal) projection
            // This means we can associate units with meters and angles are correct
            // see http://bl.ocks.org/oliverheilig/29e494c33ef58c6d5839
            var mercP = GeoTools.Wgs2SphereMercator(new Coordinate(lon, lat), true);

            // in our conformal projection we have to adopt the size depending on the latitude
            var f = 1.0 / Math.Cos((lat / 360) * 2 * Math.PI);
            radiusX *= f;
            radiusY *= f;
            buffer *= f;

            // the step size for the approximation
            var numVertices = 100;
            var darc = 2 * Math.PI / numVertices;

            // create shell
            var shell = new List<Coordinate>();
            for (var i = 0; i < numVertices; i++)
            {
                var arc = darc * i;

                var xPos = mercP.X - (radiusX * Math.Sin(arc)) * Math.Sin(rot * Math.PI) + (radiusY * Math.Cos(arc)) * Math.Cos(rot * Math.PI);
                var yPos = mercP.Y + (radiusY * Math.Cos(arc)) * Math.Sin(rot * Math.PI) + (radiusX * Math.Sin(arc)) * Math.Cos(rot * Math.PI);

                // the computed coordinates are transformed back to WGS
                shell.Add(GeoTools.SphereMercator2Wgs(new Coordinate(xPos, yPos), true));
            }
            shell.Add(shell[0]); // close ring

            // create hole
            var hole = new List<Coordinate>();
            for (var i = 0; i < numVertices; i++)
            {
                var arc = darc * i;

                var xPos = mercP.X - ((radiusX - buffer) * Math.Sin(arc)) * Math.Sin(rot * Math.PI) + ((radiusY - buffer) * Math.Cos(arc)) * Math.Cos(rot * Math.PI);
                var yPos = mercP.Y + ((radiusY - buffer) * Math.Cos(arc)) * Math.Sin(rot * Math.PI) + ((radiusX - buffer) * Math.Sin(arc)) * Math.Cos(rot * Math.PI);

                hole.Add(GeoTools.SphereMercator2Wgs(new Coordinate(xPos, yPos), true));
            }
            hole.Add(hole[0]); // close ring

            return Geometry.DefaultFactory.CreatePolygon(
                Geometry.DefaultFactory.CreateLinearRing(shell.ToArray()),
                new ILinearRing[] { Geometry.DefaultFactory.CreateLinearRing(hole.ToArray()) });
        }
    }
}
