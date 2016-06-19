using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Widgets;

namespace SharpMap.Web
{
    /// <summary>
    /// Summary description for MapInfoHandler
    /// </summary>
    public class LayerInfoHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var x = JsonConvert.SerializeObject(SampleLayers.Layers);

            context.Response.ContentType = "application/json";
            context.Response.Write(x);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}