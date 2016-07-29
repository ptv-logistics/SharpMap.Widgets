using System;
using BruTile;
using BruTile.Web;

namespace SharpMap.Print
{
    public class HereSatelliteRequest : IRequest
    {
        public string AppId { get; set; }

        public string AppCode { get; set; }

        public Uri GetUri(TileInfo info)
        {
            return new Uri(
                string.Format("http://{0}.aerial.maps.cit.api.here.com/maptile/2.1/maptile/newest/satellite.day/{1}/{2}/{3}/256/png8?app_id={4}&app_code={5}",
                1 + (info.Index.Col + info.Index.Row) % 4, info.Index.Level, info.Index.Col, info.Index.Row, AppId, AppCode));
        }
    }
}
