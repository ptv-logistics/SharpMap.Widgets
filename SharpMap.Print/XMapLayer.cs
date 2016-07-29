using System;
using System.Collections.Generic;
using System.Text;
using SharpMap.Layers;
using System.Net;
using System.IO;
using System.Net.Cache;
using GeoAPI.Geometries;
using SharpMap.Print.XMapServiceReference;

namespace SharpMap.Print
{

    /// <summary>
    /// XMapServerLayer renders a Bitmap using PTV xMapServer
    /// </summary>
    public class XMapLayer : SharpMap.Layers.Layer
    {
        private string m_Url = string.Empty;
        private MapMode m_xMapServerMode = MapMode.Background;

        public string User { get; set; }

        public string Password { get; set; }

        public float Opacity { get; set; }

        public XMapLayer(string name, string url, MapMode mode)
        {
            m_xMapServerMode = mode;
            LayerName = name;
            m_Url = url;
            SRID = 3857; // Web_MERCATOR
            Opacity = 1.0F;
        }

        public override Envelope Envelope
        {
            get
            {
                return null;
            }
        }

        public override void Render(System.Drawing.Graphics g, SharpMap.Map map)
        {
            base.Render(g, map);

            // For convenience and consistency 
            // we use SharpMap with Web- (aka Google-) Mercator. So just transform the envelope.
            var googleToPtv = 6371000.0 / 6378137.0;
            var envelope = new Envelope(map.Envelope.TopLeft().X * googleToPtv, map.Envelope.BottomRight().X * googleToPtv,
                map.Envelope.TopLeft().Y * googleToPtv, map.Envelope.BottomRight().Y * googleToPtv);

            var r = new XMapRenderer(m_Url, m_xMapServerMode){ User = User, Password = Password };
            r.DrawImage(g, (int)envelope.Left(), (int)envelope.Top(), (int)envelope.Right(), (int)envelope.Bottom(), 
                map.Size.Width, map.Size.Height, 0, Opacity);
        }
    } 
}
