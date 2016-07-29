using System;
using BruTile;
using BruTile.Web;

namespace SharpMap.Print
{
    public class XMapTwoRequests : IRequest
    {
        public string XTok { get; set; }

        public string Profile { get; set; }

        public Uri GetUri(TileInfo info)
        {
            return new Uri(
                string.Format("https://api{0}-xstwo.cloud.ptvgroup.com/services/rest/XMap/tile/{1}/{2}/{3}/{4}?xtok={5}",
                1 + (info.Index.Col + info.Index.Row) % 4, info.Index.Level, info.Index.Col, info.Index.Row, Profile, XTok));
        }
    }
}
