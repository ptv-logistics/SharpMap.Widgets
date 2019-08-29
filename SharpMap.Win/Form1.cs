using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Demo.ShapeFile;
using SharpMap.Common;
using SharpMap.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Forms;
using Widgets;
using System.Linq;

namespace SharpMap.Win
{
    public partial class Form1 : Form
    {
        // the collection of selected elements
        ObservableCollection<FeatureDataRow> selectedRegions = new ObservableCollection<FeatureDataRow>();

        public Form1()
        {
            InitializeComponent();

            // using xserver-internet
            formsMap1.XMapUrl = "https://api-eu-test.cloud.ptvgroup.com/xmap/ws/XMap";

            // this is a time-limited demo token. 
            // You need your own xServer-internet token for your application!
            formsMap1.XMapCredentials = "xtok:D5F40131-49C6-47BE-BB9E-70657D365F40";

            // set silkysand as xMap theme
            formsMap1.XMapStyle = "silkysand";

            // disable the embedded layers control
            formsMap1.ShowLayers = false;

            // attach to selection event
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

        private void CheckedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var layerName = checkedListBox1.SelectedItem.ToString();
            var layerInfo = (from li in SampleLayers.Layers where li.ToString() == layerName select li).FirstOrDefault();
            layerInfo.Visible = e.NewValue == CheckState.Checked;

            if (layerInfo.LayerCategory == LayerCategory.Point)
            {
                if (SampleLayers.Layers.GetLayers(RenderingLayer.Foreground).Count() == 0)
                    formsMap1.Layers.SetVisible(fgLayer, false);
                else
                {
                    fgLayer.Refresh();
                    formsMap1.Layers.SetVisible(fgLayer, true);
                }
            }
            else
            {
                if (SampleLayers.Layers.GetLayers(RenderingLayer.Background).Count() == 0)
                    formsMap1.Layers.SetVisible(bgLayer, false);
                else
                {
                    // bust the tile cache
                    ((SharpMapProvider)bgLayer.TiledProvider).CacheId = Guid.NewGuid().ToString();
                    bgLayer.Refresh();
                    formsMap1.Layers.SetVisible(bgLayer, true);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeCustomLayers();
        }

        UntiledLayer fgLayer;
        TiledLayer bgLayer;
        public void InitializeCustomLayers()
        {
            IEnumerable<LayerInfo> sampleInfo = SampleLayers.Layers; 

            foreach(var i in sampleInfo)
            {
                this.checkedListBox1.Items.Add(i, i.Visible);
            }

            checkedListBox1.ItemCheck += CheckedListBox1_ItemCheck;

            bgLayer = new TiledLayer("sharpmapbg")
            {
                TiledProvider = new SharpMapProvider(s => sampleInfo.GetLayers(RenderingLayer.Background, s))
                {
                    CacheId = Guid.NewGuid().ToString()
                }
            };

            formsMap1.Layers.InsertBefore(bgLayer, "Labels");

            fgLayer = new UntiledLayer("sharpmapfg")
            {
                UntiledProvider = new SharpMapProvider(s => sampleInfo.GetLayers(RenderingLayer.Foreground, s)),
            };

            formsMap1.Layers.Add(fgLayer);

            // insert layer with two canvases
            var layer = new BaseLayer("Selection")
            {
                CanvasCategories = new[] { CanvasCategory.SelectedObjects },
                CanvasFactories = new BaseLayer.CanvasFactoryDelegate[]
                {
                    m => new SelectionCanvas(m, selectedRegions)
                },
            };

            formsMap1.Layers.Add(layer);
        }
    }
}
