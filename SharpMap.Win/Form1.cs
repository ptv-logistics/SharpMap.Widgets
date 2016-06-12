using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Demo.ShapeFile;
using SharpMap.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Widgets;

namespace SharpMap.Win
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

            selectedRegions.CollectionChanged += SelectedRegions_CollectionChanged;
        }

        private void SelectedRegions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(this.selectedRegions.Count == 0)
            {
                propertyGrid1.SelectedObject = null;
                return;
            }

            var selectedItem = selectedRegions[0];

            // dump all attributes
            var d = new Dictionary<string, object>();
            foreach (DataColumn c in selectedItem.Table.Columns)
            {
                d[c.ColumnName] = selectedItem[c];
            }

            propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(d);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeCustomLayers();
        }

        public void InitializeCustomLayers()
        {
            var sharpMapLayer = new TiledLayer("sharpmapbg")
            {
                TiledProvider = new SharpMapProvider(LayerFactories.BgFactory),
            };

            this.formsMap1.Layers.InsertBefore(sharpMapLayer, "Labels");

            var fgLayer = new UntiledLayer("sharpmapfg")
            {
                UntiledProvider = new SharpMapProvider(LayerFactories.FgFactory)
            };

            this.formsMap1.Layers.Add(fgLayer);

            // insert layer with two canvases
            var layer = new BaseLayer("Selection")
            {
                CanvasCategories = new[] { CanvasCategory.SelectedObjects },
                CanvasFactories = new BaseLayer.CanvasFactoryDelegate[]
                {
                    m => new SelectionCanvas(m, selectedRegions)
                },
            };

            this.formsMap1.Layers.Add(layer);
        }
    }
}
