using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Ptv.Controls.Map.AddressMonitor;
using Ptv.XServer.Demo.Clustering;
using SharpMap.Common;
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
using System.Linq;
using NetTopologySuite.Geometries;

namespace Widgets
{
    public static class SampleLayers
    {
        /// <summary>
        /// Our collection of sample layers.
        /// </summary>
        public static IEnumerable<LayerInfo> Layers;

        /// <summary>
        /// The donut-provider represents an in-memory collection of computed shapes.
        /// </summary>
        private static SharpMap.Data.Providers.GeometryFeatureProvider donutProvider;

        static SampleLayers()
        {
            // we createa a dummy data source that contains computed in-memory shapes
            var donuts = Providers.DonutProvider.CreateRandomDonuts(100);

            var fdt = new FeatureDataTable();
            int id = 0;
            fdt.Columns.Add("Id", typeof(int));
            foreach (var donut in donuts)
            {
                var fdr = fdt.NewRow();
                fdr["Id"] = id++;
                fdr.Geometry = donut;
                fdt.AddRow(fdr);
            }

            // GeometryFeatureProvider doesn't use quadtree indexing!
            donutProvider = new SharpMap.Data.Providers.GeometryFeatureProvider(fdt);

            // initialize sample layers
            Layers = GetSampleAreas().Concat(GetPOIs()).ToList();
        }

        /// <summary>
        /// Returns a collection of background layers
        /// These layers are typically areas and are rendered tile-based
        /// </summary>
        /// <param name="pixelSize"> the optional map pixel size for dynamic scaling. </param>
        /// <returns> The collection of layers </returns>
        public static IEnumerable<LayerInfo> GetSampleAreas()
        {
            var x = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Shape", "world_countries_boundary_file_world_2002.shp");

            // word countries (colored by population density)
            yield return new LayerInfo
            {
                Name = "WorldCountries",
                LayerCategory = LayerCategory.Area,
                Visible = true,
                LayerFactory = (theme, pixelSize) =>
                new VectorLayer("WorldCountries")
                {
                    // set tranform to WGS84->Spherical_Mercator
                    CoordinateTransformation = new CoordinateTransformationFactory().CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WebMercator),

                    // set the sharpmap provider for shape files as data source
                    DataSource = new SharpMap.Data.Providers.ShapeFile(x, false, true),

                    // use a dynamic style for thematic mapping
                    // the lambda also takes the map instance into account (to scale the border width)
                    Theme = new CustomTheme(row => GetPopDensStyle(pixelSize, row))
                }
            };

            // computed shapes ("Donuts")
            yield return new LayerInfo
            {
                Name = "Donuts",
                LayerCategory = LayerCategory.Area,
                Visible = true,
                LayerFactory = (theme, pixelSize) =>
                new VectorLayer("Donuts")
                {
                    // set tranform to WGS84->Spherical_Mercator
                    CoordinateTransformation = new CoordinateTransformationFactory().CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WebMercator),

                    // set the sharpmap provider for shape files as data source
                    DataSource = donutProvider,

                    // use a dynamic style for thematic mapping
                    // the lambda also takes the map instance into account (to scale the border width)
                    Theme = new CustomTheme(row => GetDonutStyle(pixelSize, row))
                }
            };
        }

        /// <summary>
        /// Returns a collection of foreground layers
        /// These layers are typically symbols and are rendered viewport-based
        /// </summary>
        /// <param name="pixelSize"> the optional map pixel size for dynamic scaling. </param>
        /// <returns> The collection of layers </returns>
        public static IEnumerable<LayerInfo> GetPOIs()
        {
            // add wikipedia pois
            var fdt = new FeatureDataTable();
            fdt.Columns.Add("Id", typeof(int));
            fdt.Columns.Add("Name", typeof(string));


            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "POI",
                "wikilocations.csv");
            using (var reader = new CsvFileReader(filePath, (char) 0x09))
            {
                int id = 0;

                var row = new CsvRow();
                while (reader.ReadRow(row))
                {
                    if (row.Count < 3)
                        continue;

                    double x, y;
                    bool parsed = Double.TryParse(row[2], NumberStyles.Float, CultureInfo.InvariantCulture, out x);
                    x = parsed ? x : Double.NaN;
                    parsed = Double.TryParse(row[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                    y = parsed ? y : Double.NaN;

                    var fdr = fdt.NewRow();
                    fdr["Id"] = id++;
                    fdr["Name"] = row[0];
                    fdr.Geometry = new NetTopologySuite.Geometries.Point(x, y);
                    fdt.AddRow(fdr);
                }

            }
            var wikiProvider = new SharpMap.Data.Providers.GeometryFeatureProvider(fdt);
            var bitmap = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "POI", "Bitmaps", "wikipedia.png"));

            yield return new LayerInfo
            {
                Name = "Wiki",
                LayerCategory = LayerCategory.Point,
                Visible = true,
                LayerFactory = (theme, pixelSize) =>
                new VectorLayer("Wiki")
                {
                    // set tranform to WGS84->Spherical_Mercator
                    CoordinateTransformation = new CoordinateTransformationFactory().CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WebMercator),
               
                    // set the sharpmap provider for shape files as data source
                    DataSource = wikiProvider,
                                
                    // use a dynamic style for thematic mapping
                    // the lambda also takes the map instance into account (to scale the border width)
                    Theme = new CustomTheme(row => new VectorStyle() { Symbol = bitmap }),

                    // display threshold
                    MaxVisible = 7500
                }
            };

            // find all poi-databases
            var rootPath = AppDomain.CurrentDomain.BaseDirectory + "Data\\Poi";
            var bitmapPath = rootPath + "\\Bitmaps";
            string[] poiFiles = Directory.GetFiles(rootPath, "*.mdb");

            foreach (string poiFile in poiFiles)
            {
                yield return new LayerInfo
                {
                    Name = Path.GetFileNameWithoutExtension(poiFile),
                    LayerCategory = LayerCategory.Point,
                    Visible = true,
                    Caption = AMLayerFactory.GetVisibleName(poiFile),
                    LayerFactory = (theme, pixelSize) =>
                    {
                        var layer = AMLayerFactory.CreateLayer(poiFile, bitmapPath);
                        layer.MaxVisible = 9999999999; // override the automatic display threshold
                        return layer;
                    }
                };
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
            fillColor = Color.FromArgb(128, fillColor.R, fillColor.G, fillColor.B);

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

            // set the border width depending on the map scale
            var pen = new Pen(Brushes.Black, (int)(50.0 / pixelSize)) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round };

            return new VectorStyle { Outline = pen, EnableOutline = true, Fill = brush };
        }
    }
}
