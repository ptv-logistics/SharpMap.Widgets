//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using GeoAPI.Geometries;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Tools;
using SharpMap.Common;
using SharpMap.Data;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ToursAndStops;

namespace Widgets
{
    /// <summary>
    /// This class is responsible for highlighting selected shapes.
    /// </summary>
    public class SelectionCanvas : WorldCanvas
    {
        #region private variables
        /// <summary>Holds the selected geometries.</summary>
        private readonly ObservableCollection<FeatureDataRow> geometries;
        /// <summary>The shape file used to read data for selection highlighting.</summary>
        #endregion

        /// <summary> Initializes a new instance of the <see cref="ShapeSelectionCanvas"/> class. Creates a new
        /// instance of the given shape file. </summary>
        /// <param name="mapView"> The map. </param>
        /// <param name="shapeFilePath"> The absolute path to the shape file. </param>
        /// <param name="geometries"> Holds the geometries to highlight. </param>
        public SelectionCanvas(MapView mapView, ObservableCollection<FeatureDataRow> geometries)
            : base(mapView)
        {
            this.geometries = geometries;
            geometries.CollectionChanged += geometries_CollectionChanged;
            renderTransform = new ScaleTransform(MapView.CurrentScale, MapView.CurrentScale);

            if (MapView.Name == "Map") // only select on main map
            {
                MapView.MouseLeftButtonUp += map_MouseUp;
                MapView.MouseMove += MapView_MouseMove;
                MapView.MouseLeftButtonDown += MapView_MouseLeftButtonDown;
            }
        }

        private bool indSelect = false;
        private Point clickPoint;

        private void MapView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            indSelect = true;
            clickPoint = e.GetPosition(MapView);
        }

        private void MapView_MouseMove(object sender, MouseEventArgs e)
        {
            if (!indSelect)
                return;

            var a = e.GetPosition(MapView);
            if(Math.Abs(clickPoint.X - a.X) > 4 || Math.Abs(clickPoint.Y - a.Y) > 4)
                indSelect = false;
        }

        #region disposal
        /// <inheritdoc/>
        public override void Dispose()
        {
            geometries.CollectionChanged -= geometries_CollectionChanged;

            if (MapView.Name == "Map") // only select on main map
            {
                MapView.MouseLeftButtonUp -= map_MouseUp;
                MapView.MouseMove -= MapView_MouseMove;
                MapView.MouseLeftButtonDown -= MapView_MouseLeftButtonDown;
            }

            base.Dispose();
        }
        #endregion

        #region event handling
        /// <summary>  Notifies about a change of the selection set.  </summary>
        /// <param name="sender"> The sender of the CollectionChanged event. </param>
        /// <param name="e"> The event parameters. </param>
        private void geometries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateSelection();
        }

        /// <summary> Event handler for MouseDown event.  </summary>
        /// <param name="sender"> The sender of the MouseDown event. </param>
        /// <param name="e"> The event parameters. </param>
        #region doc:map_MouseDown handler
        private void map_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //if (!(e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed))
            //    return;
            if (!indSelect)
                return;

            indSelect = false;

            geometries.Clear();

            var canvasPoint = e.GetPosition(this);
            System.Windows.Point wgsPoint = CanvasToGeo(canvasPoint);

            var row = HitTester.HitTest(SampleLayers.Layers, wgsPoint.Y, wgsPoint.X, this.MapView.CurrentZoom);

            if (row != null)
                geometries.Add(row);
        }
        #endregion //doc:map_MouseDown handler
        #endregion

        private ScaleTransform renderTransform;

        #region update methods
        /// <summary> Updates the selected objects set. </summary>
        #region doc:UpdateSelection method
        private void UpdateSelection()
        {
            Children.Clear();

            foreach (var fdr in geometries)
            {
                if (fdr.Geometry is IPoint)
                {
                    var point = (IPoint)fdr.Geometry;
                    var cp = GeoToCanvas(new Point(point.X, point.Y));

                    var balloon = new Balloon();
                    balloon.Color = System.Windows.Media.Colors.Blue;
                    Children.Add(balloon);
                    balloon.UpdateLayout();

                    SetLeft(balloon, cp.X - balloon.ActualWidth / 2);
                    SetTop(balloon, cp.Y - balloon.ActualHeight / 2);

                    balloon.RenderTransform = renderTransform;
                    balloon.RenderTransformOrigin = new Point(.5, .5);
                }
                else
                {
                    var geometry = WkbToWpf.Parse(fdr.Geometry.AsBinary(), GeoToCanvas);

                    Children.Add(new System.Windows.Shapes.Path
                    {
                        Fill = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color { A = 192, R = 255, G = 255, B = 255 }),
                        Stroke = System.Windows.Media.Brushes.Black,
                        StrokeLineJoin = System.Windows.Media.PenLineJoin.Round,
                        StrokeThickness = 4 * MapView.FinalScale,
                        Data = geometry
                    });
                }
            }
        }
        #endregion //doc:UpdateSelection method

        /// <inheritdoc/>
        public override void Update(UpdateMode updateMode)
        {
            if (updateMode == UpdateMode.Refresh || updateMode == UpdateMode.EndTransition)
                UpdateSelection();

            renderTransform.ScaleX = renderTransform.ScaleY = MapView.CurrentScale;
        }
        #endregion
    }
}
