using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.TileProviders;
using SharpMap.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
            formsMap1.XMapCredentials = "xtok:953B0471-1EB8-4E1C-B170-ACDF1B04D6B5";

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

            bgLayer = new TiledLayer("sharpmapbg")
            {
                TiledProvider = new RemoteTiledProvider
                {
                    MinZoom = 0,
                    MaxZoom = 20,
                    RequestBuilderDelegate = (x, y, level) =>
                        string.Format(
                            "http://localhost:60811/SharpMapTilesHandler.ashx?x={1}&y={2}&z={0}", level, x, y)
                },
            };

            formsMap1.Layers.InsertBefore(bgLayer, "Labels");

           fgLayer = new UntiledLayer("sharpmapfg")
            {
                Caption = "Bus Stops",
                UntiledProvider = new WmsProvider("http://localhost:60811/SharpMapOverlayHandler.ashx")
            };

            formsMap1.Layers.Add(fgLayer);
        }
    }
}
