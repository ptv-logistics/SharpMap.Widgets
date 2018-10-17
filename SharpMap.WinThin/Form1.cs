using Newtonsoft.Json;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.TileProviders;
using SharpMap.Common;
using SharpMap.Data;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace SharpMap.WinThin
{
    public partial class Form1 : Form
    {
        // the collection of selected elements
        ObservableCollection<FeatureDataRow> selectedRegions = new ObservableCollection<FeatureDataRow>();

        public Form1()
        {
            InitializeComponent();

            // using infinite-zoom, makes the widget less jaggy at deep zoom levels
            Ptv.XServer.Controls.Map.GlobalOptions.InfiniteZoom = true;

            // using xserver-internet
            formsMap1.XMapUrl = "https://api-eu-test.cloud.ptvgroup.com/xmap/ws/XMap";

            // this is a time-limited demo token. 
            // You need your own xServer-internet token for your application!
<<<<<<< HEAD
            formsMap1.XMapCredentials = "xtok:EBB3ABF6-C1FD-4B01-9D69-349332944AD9";
=======
            formsMap1.XMapCredentials = "xtok:9358789A-A8CF-4CA8-AC99-1C0C4AC07F1E";
>>>>>>> b0fca96df3b65fb8b551eff9dd9e6b5c1b6b83c8

            // set silkysand as xMap theme
            formsMap1.XMapStyle = "silkysand";

            // disable the embedded layers control
            formsMap1.ShowLayers = false;

            // attach to selection event
            selectedRegions.CollectionChanged += SelectedRegions_CollectionChanged;
        }

        private void SelectedRegions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        private void CheckedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeCustomLayers();
        }

        UntiledLayer fgLayer;
        TiledLayer bgLayer;
        public void InitializeCustomLayers()
        {
            var str = new WebClient().DownloadString("http://localhost:60811/LayerInfoHandler.ashx");
            var layerInfos = JsonConvert.DeserializeObject<LayerInfo[]>(str);

            var fgLayers = from l in layerInfos where l.LayerCategory == LayerCategory.Point select l;
            var bgLayers = from l in layerInfos where l.LayerCategory != LayerCategory.Point select l;

            bgLayer = new TiledLayer("sharpmapbg")
            {
                TiledProvider = new RemoteTiledProvider
                {
                    MinZoom = 0,
                    MaxZoom = 20,
                    RequestBuilderDelegate = (x, y, level) =>
                        string.Format(
                            "http://localhost:60811/SharpMapTilesHandler.ashx?x={1}&y={2}&z={0}&layers={3}", 
                            level, x, y, string.Join(",", from l in bgLayers select l.Name))
                },
            };

            formsMap1.Layers.InsertBefore(bgLayer, "Labels");

            fgLayer = new UntiledLayer("sharpmapfg")
            {
                Caption = "Bus Stops",
                UntiledProvider = new WmsProvider(string.Format("http://localhost:60811/SharpMapOverlayHandler.ashx?layers={0}",
                    string.Join(",", from l in fgLayers select l.Name)))
            };

            formsMap1.Layers.Add(fgLayer);
        }
    }
}