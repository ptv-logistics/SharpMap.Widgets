using System;
using System.Globalization;
using System.IO;
using System.Net;
using Ptv.XServer.Controls.Map.Layers.Untiled;

namespace SharpMap.WinThin
{
    /// <summary>
    /// A minimal provider that doeas WMS requests
    /// </summary>
    internal class WmsProvider : IUntiledProvider
    {
        private string baseUrl;
    
        public WmsProvider(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }
        
        public Stream GetImageStream(double left, double top, double right, double bottom, int width, int height)
        {
            // the bounds is ptv mercator, have convert it to Web-Mercator for WMS
            var ptvToGoogle = 6378137.0 / 6371000.0;

            var requestString = string.Format(CultureInfo.InvariantCulture,
                "{0}?service=WMS&request=GetMap&version=1.1.1&layers=&styles=format=image/png&transparent=true&srs=EPSG:3857&bbox={1},{2},{3},{4}&width={5}&height={6}",
                baseUrl, left* ptvToGoogle, top* ptvToGoogle, right* ptvToGoogle, bottom* ptvToGoogle, width, height);

            var request = WebRequest.Create(requestString);

            return request.GetResponse().GetResponseStream();
        }
    }
}