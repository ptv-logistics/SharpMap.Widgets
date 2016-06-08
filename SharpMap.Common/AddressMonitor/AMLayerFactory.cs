using System;
using System.Collections.Generic;
using System.Text;
using SharpMap.Layers;
using System.Drawing;
using System.Data.OleDb;
using Ptv.Controls.Map;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;

namespace Ptv.Controls.Map.AddressMonitor
{
    public class AMLayerFactory
    {
        public static ILayer CreateLayer(string fileName, string bitmapBase)
        {
            var layer = new VectorLayer(System.IO.Path.GetFileNameWithoutExtension(fileName));

            // initialize data source
            AMProvider pointProv = new AMProvider(
                @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName,
                "_IndexData", "AUTOINC", "X", "Y");
            layer.DataSource = pointProv;

            layer.CoordinateTransformation = new CoordinateTransformationFactory().CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WebMercator);

            // initialize style
            AMStyle amStyle = new AMStyle(fileName, bitmapBase);
            if(amStyle.MaxVisible > 0)
                layer.MaxVisible = amStyle.MaxVisible;
            layer.Theme = amStyle;
            
            return layer;
        }
    }
}
