using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Ptv.Controls.Map.AddressMonitor;
using SharpMap.Data;
using SharpMap.Layers;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace Widgets
{
    public static class LayerFactories
    {
        private static SharpMap.Data.Providers.GeometryFeatureProvider donutProvider;
        static LayerFactories()
        {
            // we createa a dummy data source that contains computed in-memory shapes
            var donuts = DonutProvider.CreateShapes(100);

            var fdt = new FeatureDataTable();
            int id = 0;
            fdt.Columns.Add("Id", typeof(Int32));
            foreach (var donut in donuts)
            {
                var fdr = fdt.NewRow();
                fdr["Id"] = id++;
                fdr.Geometry = donut;
                fdt.AddRow(fdr);
            }

            // todo: check if the GeometryFeatureProvider uses some quadtree indexing
            donutProvider = new SharpMap.Data.Providers.GeometryFeatureProvider(fdt);
        }

        /// <summary>
        /// Returns a collection of background layers
        /// These layers are typically areas and are rendered tile-based
        /// </summary>
        /// <param name="pixelSize"> the optional map pixel size for dynamic scaling. </param>
        /// <returns> The collection of layers </returns>
        public static IEnumerable<SharpMap.Layers.ILayer> BgFactory(double pixelSize = 1)
        {
            var x = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Shape", "world_countries_boundary_file_world_2002.shp");

            // the map contains only one layer
            yield return new VectorLayer("WorldCountries")
            {
                // set tranform to WGS84->Spherical_Mercator
                CoordinateTransformation = new CoordinateTransformationFactory().CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WebMercator),

                // set the sharpmap provider for shape files as data source
                DataSource = new SharpMap.Data.Providers.ShapeFile(x, false, true),

                // use a dynamic style for thematic mapping
                // the lambda also takes the map instance into account (to scale the border width)
                Theme = new CustomTheme(row => GetPopDensStyle(pixelSize, row))
            };

            // the map contains only one layer
            yield return new VectorLayer("Donuts")
            {
                // set tranform to WGS84->Spherical_Mercator
                CoordinateTransformation = new CoordinateTransformationFactory().CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WebMercator),

                // set the sharpmap provider for shape files as data source
                DataSource = donutProvider,

                // use a dynamic style for thematic mapping
                // the lambda also takes the map instance into account (to scale the border width)
                Theme = new CustomTheme(row => GetDonutStyle(pixelSize, row))
            };
        }

        /// <summary>
        /// Returns a collection of foreground layers
        /// These layers are typically symbols and are rendered viewport-based
        /// </summary>
        /// <param name="pixelSize"> the optional map pixel size for dynamic scaling. </param>
        /// <returns> The collection of layers </returns>
        public static IEnumerable<SharpMap.Layers.ILayer> FgFactory(double pixelSize = 1)
        {
            // insert address monitor layers
            var rootPath = System.AppDomain.CurrentDomain.BaseDirectory + "Data\\Poi";
            var bitmapPath = rootPath + "\\Bitmaps";
            string[] poiFiles = System.IO.Directory.GetFiles(rootPath, "*.mdb");
            foreach (string poiFile in poiFiles)
            {
                var layer = AMLayerFactory.CreateLayer(poiFile, bitmapPath);
                layer.MaxVisible = 9999999999; // override the automatic display threshold
                yield return layer;
            }
        }

        #region doc:GetPopDensStyle method
        /// <summary> Demonstrates the use of dynamic styles (themes) for vector layers. In this case we 
        /// use the population density to color the shape to draw.</summary>
        /// <param name="pixelSize"> The pixel size of the map. </param>
        /// <param name="row"> The currently processed data row. </param>
        /// <returns> A VectorStyle which is used to style the shape to draw. </returns>
        private static VectorStyle GetPopDensStyle(double pixelSize, DataRow row)
        {
            float scale;

            try
            {
                // colorize the polygon according to buying power;
                var pop = Convert.ToDouble(row["POP2005"], NumberFormatInfo.InvariantInfo);
                var area = Convert.ToDouble(row["AREA"], NumberFormatInfo.InvariantInfo);
                // compute a scale [0..1] for the population density
                scale = (float)((area > 0) ? Math.Min(1.0, Math.Sqrt(pop / area) / 70) : -1.0f);
            }
            catch (Exception)
            {
                scale = -1.0f;
            }

            Color fillColor;
            if (scale < 0)
                fillColor = Color.Gray;
            else
                // use the sharpmap ColorBlend for a color gradient green->yellow->red
                fillColor = ColorBlend.ThreeColors(Color.Green, Color.Yellow, Color.Red).GetColor(scale);

            // make fill color alpha-transparent
            fillColor = Color.FromArgb(180, fillColor.R, fillColor.G, fillColor.B);

            // set the border width depending on the map scale
            var pen = new Pen(Brushes.Black, (int)(50.0 / pixelSize)) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round };

            return new VectorStyle { Outline = pen, EnableOutline = true, Fill = new SolidBrush(fillColor) };
        }
        #endregion //doc:GetPopDensStyle method

        // returns the style for our donuts
        private static VectorStyle GetDonutStyle(double pixelSize, DataRow row)
        {
            var colors = new[] { System.Drawing.Color.Blue, System.Drawing.Color.Green, System.Drawing.Color.Red };

            var brush = new System.Drawing.Drawing2D.HatchBrush(
                System.Drawing.Drawing2D.HatchStyle.DiagonalCross, colors[((int)row["Id"]) % colors.Length], 
                Color.White);

            // make fill color alpha-transparent
//            fillColor = Color.FromArgb(180, fillColor.R, fillColor.G, fillColor.B);

            // set the border width depending on the map scale
            var pen = new Pen(Brushes.Black, (int)(50.0 / pixelSize)) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round };

            return new VectorStyle { Outline = pen, EnableOutline = true, Fill = brush };
        }
    }
}
