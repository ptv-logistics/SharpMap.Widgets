using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using SharpMap.Layers;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Ptv.Controls.Map.AddressMonitor
{
    public class AMLayerFactory
    {
        public static ILayer CreateLayer(string fileName, string bitmapBase)
        {
            var layer = new VectorLayer(Path.GetFileNameWithoutExtension(fileName));

            // initialize data source
            var pointProv = new AMProvider(
                @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName,
                "_IndexData", "AUTOINC", "X", "Y");
            layer.DataSource = pointProv;

            layer.CoordinateTransformation = new CoordinateTransformationFactory().CreateFromCoordinateSystems(
                GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WebMercator);

            // initialize style
            var amStyle = new AMStyle(fileName, bitmapBase);
            if(amStyle.MaxVisible > 0)
                layer.MaxVisible = amStyle.MaxVisible;
            layer.Theme = amStyle;
            
            return layer;
        }

        public static string GetVisibleName(string fileName)
        {
            var path = Path.GetDirectoryName(fileName) + @"\" + Path.GetFileNameWithoutExtension(fileName) + ".ini";
            var data = File.ReadAllText(path);

            string pattern = @"
^                           # Beginning of the line
((?:\[)                     # Section Start
     (?<Section>[^\]]*)     # Actual Section text into Section Group
 (?:\])                     # Section End then EOL/EOB
 (?:[\r\n]{0,}|\Z))         # Match but don't capture the CRLF or EOB
 (                          # Begin capture groups (Key Value Pairs)
  (?!\[)                    # Stop capture groups if a [ is found; new section
  (?<Key>[^=]*?)            # Any text before the =, matched few as possible
  (?:=)                     # Get the = now
  (?<Value>[^\r\n]*)        # Get everything that is not an Line Changes
  (?:[\r\n]{0,4})           # MBDC \r\n
  )+                        # End Capture groups";

            Dictionary<string, Dictionary<string, string>> InIFile
            = (from Match m in Regex.Matches(data, pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline)
               select new
               {
                   Section = m.Groups["Section"].Value,

                   kvps = (from cpKey in m.Groups["Key"].Captures.Cast<Capture>().Select((a, i) => new { a.Value, i })
                           join cpValue in m.Groups["Value"].Captures.Cast<Capture>().Select((b, i) => new { b.Value, i }) on cpKey.i equals cpValue.i
                           select new KeyValuePair<string, string>(cpKey.Value, cpValue.Value)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)

               }).ToDictionary(itm => itm.Section, itm => itm.kvps);

            return InIFile["AdrMon " + Path.GetFileNameWithoutExtension(fileName)]["VisibleName"].Trim();
        }
    }
}
