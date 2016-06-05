using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace Widgets
{
    /// <summary>
    /// The donut provider creates an in-memory set of computed shapes and stores them as OCG binaries
    /// inside a quad-tree. This is just a template to use your own computed shapes.
    /// </summary>
    public class DonutProvider
    {   
        // re-create ou shapes
        public static IEnumerable<GeoAPI.Geometries.IGeometry> CreateShapes(int number)
        {
            // we just generate some random objects
            var rand = new Random();

            for (var dix = 0; dix < number; dix++)
            {
                // our donut parameters
                var lat = rand.NextDouble() * 4 + 45.0; // latitude
                var lon = rand.NextDouble() * 6 + 0.0; // logitude
                var rot = rand.NextDouble() * Math.PI; // rotation angle
                var radiusX = rand.NextDouble() * 20000.0 + 10000.0; // x-radius in m
                var radiusY = rand.NextDouble() * 20000.0 + 10000.0; // y-radius in m
                var buffer = 10000.0; // buffer size in m

                // the donut shapes are calulcated in a mercator (= conformal) projection
                // This means we can assiciate units with meters and angles are correct
                // see http://bl.ocks.org/oliverheilig/29e494c33ef58c6d5839
                var mercP = Ptv.Controls.Map.AddressMonitor.AMProvider.Wgs2SphereMercator(new Coordinate(lon, lat));

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

                    shell.Add(Ptv.Controls.Map.AddressMonitor.AMProvider.SphereMercator2Wgs(new Coordinate(xPos, yPos)));
                }
                shell.Add(shell[0]); // close ring

                // create hole
                var hole = new List<Coordinate>();
                for (var i = 0; i < numVertices; i++)
                {
                    var arc = darc * i;

                    var xPos = mercP.X - ((radiusX - buffer) * Math.Sin(arc)) * Math.Sin(rot * Math.PI) + ((radiusY - buffer) * Math.Cos(arc)) * Math.Cos(rot * Math.PI);
                    var yPos = mercP.Y + ((radiusY - buffer) * Math.Cos(arc)) * Math.Sin(rot * Math.PI) + ((radiusX - buffer) * Math.Sin(arc)) * Math.Cos(rot * Math.PI);

                    hole.Add(Ptv.Controls.Map.AddressMonitor.AMProvider.SphereMercator2Wgs(new Coordinate(xPos, yPos)));
                }
                hole.Add(hole[0]); // close ring

                yield return Geometry.DefaultFactory.CreatePolygon(
                    Geometry.DefaultFactory.CreateLinearRing(shell.ToArray()),
                    new ILinearRing[] { Geometry.DefaultFactory.CreateLinearRing(hole.ToArray()) });
            }
        }
    }
}
