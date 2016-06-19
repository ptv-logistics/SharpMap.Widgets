using Newtonsoft.Json;
using SharpMap.Layers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpMap.Common
{
    public enum LayerCategory
    {
        Point,
        Line,
        Area
    }

    public enum RenderingLayer
    {
        Background,
        Foreground
    }

    public class LayerInfo
    {
        public string Name { get; set; }

        public string Caption { get; set; }

        public LayerCategory LayerCategory { get; set; }

        public string[] Themes { get; set; }

        [JsonIgnore]
        public bool Visible { get; set; }

        [JsonIgnore]
        public string CurrentTheme { get; set; }

        [JsonIgnore]
        public Func<string, double, ILayer> LayerFactory { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Caption) ? Name : Caption;
        }
    }

    public static class LayerInfoExt
    {
        public static IEnumerable<ILayer> GetLayers(this IEnumerable<LayerInfo> layers, RenderingLayer renderingLayer, double pixelSize = 1)
        {
            foreach (var layer in from l in layers where l.Visible select l)
            {
                if (renderingLayer == layer.GetRenderingLayer())
                    yield return layer.LayerFactory(layer.CurrentTheme, pixelSize);
            }
        }

        public static RenderingLayer GetRenderingLayer(this LayerInfo layerInfo)
        {
            return layerInfo.LayerCategory == LayerCategory.Point ? RenderingLayer.Foreground : RenderingLayer.Background;
        }
    }
}
