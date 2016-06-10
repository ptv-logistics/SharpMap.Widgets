using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Demo.ShapeFile;
using SharpMap.Data;
using System.Collections.ObjectModel;
using System.Windows;

namespace Widgets
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            // using infinite-zoom, makes the widget less jaggy at deep zoom levels
            Ptv.XServer.Controls.Map.GlobalOptions.InfiniteZoom = true;

            InitializeComponent();

            // using xserver-internet
            Map.XMapUrl = "https://api-eu-test.cloud.ptvgroup.com/xmap/ws/XMap";

            // this is a time-limited demo token. 
            // You need your own xServer-internet token for your application!
            Map.XMapCredentials = "xtok:953B0471-1EB8-4E1C-B170-ACDF1B04D6B5";

            this.Map.Loaded += Map_Loaded;
        }

        // the collection of selected elements
        ObservableCollection<FeatureDataRow> selectedRegions = new ObservableCollection<FeatureDataRow>();

        void Map_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCustomLayer();
        }

        public void InitializeCustomLayer()
        {
            var sharpMapLayer = new TiledLayer("sharpmapbg")
            {
                TiledProvider = new SharpMapProvider(LayerFactories.BgFactory),
            };

            this.Map.Layers.InsertBefore(sharpMapLayer, "Labels");

            var fgLayer = new UntiledLayer("sharpmapfg")
            {
                UntiledProvider = new SharpMapProvider(LayerFactories.FgFactory)
            };

            this.Map.Layers.Add(fgLayer);

            // insert layer with two canvases
            var layer = new BaseLayer("Selection")
            {
                CanvasCategories = new[] { CanvasCategory.SelectedObjects },
                CanvasFactories = new BaseLayer.CanvasFactoryDelegate[]
                {
                    m => new SelectionCanvas(m, selectedRegions)
                },
            };

            this.Map.Layers.Add(layer);
        }
    }
}