using System;
using System.Collections.Generic;
using System.Text;

namespace Ptv.Controls.Map.AddressMonitor
{
    public class AMLocator
    {
        public static void Locate(SharpMap.Map map, string rootPath, string bitmapPath)
        {
            string[] poiFiles = System.IO.Directory.GetFiles(rootPath, "*.poi");
            foreach (string poiFile in poiFiles)
            {
                map.Layers.Add(AMLayerFactory.CreateLayer(poiFile, bitmapPath));
            }
        }
    }
}
