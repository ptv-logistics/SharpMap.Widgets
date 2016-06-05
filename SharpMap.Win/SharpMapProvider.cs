using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Ptv.XServer.Demo.ShapeFile
{
    /// <summary> <para>Demonstrates how SharpMap can be used to implement a layer which uses shape files as data source.</para>
    /// <para>See the <conceptualLink target="427ab62e-f02d-4e92-9c26-31e0f89d49c5"/> topic for an example.</para> </summary>
    public class SharpMapProvider : ITiledProvider, IUntiledProvider
    {
        /// Web mercator earth radius. 
        public const double EarthRadius = 6378137.0;

        #region static methods
        /// <summary> Calculates a Mercator bounding box for a tile key. </summary>
        /// <param name="tileX"> The tile x coordinate in PTV-internal format. </param>
        /// <param name="tileY"> The tile y coordinate in PTV-internal format. </param>
        /// <param name="zoom"> The zoom level. </param>
        /// <returns> A bounding box in Mercator format which corresponds to the given tile coordinates and zoom level. </returns>
        public static Envelope TileToWebMercatorAtZoom(int tileX, int tileY, int zoom)
        {
            const double earthCircum = EarthRadius * 2.0 * Math.PI;
            const double earthHalfCircum = earthCircum / 2;
            double arc = earthCircum / (1 << zoom);

            return new Envelope(
                (tileX * arc) - earthHalfCircum, ((tileX + 1) * arc) - earthHalfCircum,
                earthHalfCircum - ((tileY + 1) * arc), earthHalfCircum - (tileY * arc));
        }

        /// <summary>
        /// Creates a transformer which transforms coordinates from the given source coordinate system to Mercator.
        /// </summary>
        /// <param name="source">The source coordinate system.</param>
        /// <returns>The coordinate transformer.</returns>
        public static ICoordinateTransformation TransformToMercator(ICoordinateSystem source)
        {
            var ctFact = new CoordinateTransformationFactory();
            return ctFact.CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WebMercator);
        }
        #endregion

        /// <summary>The data source containing the shape files.</summary>
        private readonly Func<double, IEnumerable<SharpMap.Layers.ILayer>> sharpMapFactory;

        /// <summary> Initializes a new instance of the <see cref="SharpMapTiledProvider"/> class. The SharpMap tiled
        /// provider reads the shapes from the given shape file path. </summary>
        /// <param name="shapeFile"> The full qualified path to the shape file. </param>
        public SharpMapProvider(Func<double, IEnumerable<SharpMap.Layers.ILayer>> sharpMapFactory)
        {
            this.sharpMapFactory = sharpMapFactory; 
        }

        #region Implementation of ITiledProvider
        #region doc:GetImageStream method

        /// <inheritdoc/>
        public Stream GetImageStream(int x, int y, int zoom)
        {
            // convert the tile key to a mercator envelope and render the image
            return GetImageStream(TileToWebMercatorAtZoom(x, y, zoom), 256, 256);
        }

        /// <inheritdoc/>
        public Stream GetImageStream(double left, double top, double right, double bottom, int width, int height)
        {
            var envelope = new Envelope(left, right, top, bottom);

            // The bounds for GetImageStream are PTV-Mercator, but for convenience and consistency 
            // we use SharpMap with Web- (aka Google-) Mercator. So just transform the envelope.
            var ptvToGoogle = 6378137.0 / 6371000.0;
            envelope.ExpandBy(ptvToGoogle, ptvToGoogle);

            // now render the imate
            return GetImageStream(envelope, width, height);
        }

        public Stream GetImageStream(Envelope envelope, int width, int height)
        {
            // create a transparent sharpmap map with a size of 256x256
            using (var sharpMap = new SharpMap.Map(new Size(width, height)) { BackColor = Color.Transparent })
            {
                // calculate the bbox for the tile key and zoom the map 
                sharpMap.ZoomToBox(envelope);

                // add the layer to the map
                foreach (var l in sharpMapFactory(sharpMap.PixelSize))
                    sharpMap.Layers.Add(l);

                // render the map image
                using (var img = sharpMap.GetMap())
                {
                    // stream the image to the client
                    var memoryStream = new MemoryStream();
                            
                    // Saving a PNG image requires a seekable stream, first save to memory stream 
                    // http://forums.asp.net/p/975883/3646110.aspx#1291641
                    img.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream;
                }
            }
        }

        #endregion // doc:GetImageStream method

        /// <inheritdoc/>
        public string CacheId { get { return "SharpMapTiledProvider"; } }

        /// <inheritdoc/>
        public int MaxZoom { get { return 19; } }

        /// <inheritdoc/>
        public int MinZoom { get { return 0; } }
        #endregion
    }
}
