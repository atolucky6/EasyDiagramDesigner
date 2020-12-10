using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyDiagram.Core
{
    public class ConnectionAdorner : Adorner
    {
        #region Constructors
        public ConnectionAdorner(DesignerCanvas designerCanvas, Connection connection) : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _adornerCanvas = new Canvas();
            _visualChildren = new VisualCollection(this);
            _visualChildren.Add(_adornerCanvas);

            _connection = connection;
            _connection.PropertyChanged += AnchorPositionChanged;

            InitializeDragThumbs();

            _drawingPen = new Pen(Brushes.LightSlateGray, 1);
            _drawingPen.LineJoin = PenLineJoin.Round;

            Unloaded += OnUnloaded;
        }
        #endregion

        #region Properties
        public DesignerItem HitDesignerItem
        {
            get => _hitDesignerItem;
            set
            {
                if (_hitDesignerItem != value)
                {
                    if (_hitDesignerItem != null)
                    {
                        _hitDesignerItem.IsDragConnectionOver = false;
                    }

                    _hitDesignerItem = value;

                    if (_hitDesignerItem != null)
                    {
                        _hitDesignerItem.IsDragConnectionOver = true;
                    }
                }
            }
        }

        public Connector HitConnector
        {
            get => _hitConnector;
            set
            {
                if (_hitConnector != value)
                {
                    _hitConnector = value;
                }
            }
        }

        protected override int VisualChildrenCount => _visualChildren.Count;
        #endregion

        #region Members
        DesignerCanvas _designerCanvas;
        Canvas _adornerCanvas;
        Connection _connection;
        PathGeometry _pathGeometry;
        Connector _fixConnector;
        Connector _dragConnector;
        Thumb _sourceDragThumb;
        Thumb _sinkDragThumb;
        Pen _drawingPen;
        DesignerItem _hitDesignerItem;
        Connector _hitConnector;
        VisualCollection _visualChildren;
        #endregion

        #region Event handlers
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _sinkDragThumb.DragDelta -= OnThumbDragDelta;
            _sinkDragThumb.DragStarted -= OnThumbDragStarted;
            _sinkDragThumb.DragCompleted -= OnThumbDragCompleted;

            _sourceDragThumb.DragDelta -= OnThumbDragDelta;
            _sourceDragThumb.DragStarted -= OnThumbDragStarted;
            _sourceDragThumb.DragCompleted -= OnThumbDragCompleted;
        }

        private void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (HitConnector != null)
            {
                if (_connection != null)
                {
                    if (_connection.Source == _fixConnector)
                        _connection.Sink = this.HitConnector;
                    else
                        _connection.Source = this.HitConnector;
                }
            }

            HitDesignerItem = null;
            HitConnector = null;
            _pathGeometry = null;
            _connection.StrokeDashArray = null;
            InvalidateVisual();
        }

        private void OnThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            HitDesignerItem = null;
            HitConnector = null;
            _pathGeometry = null;
            Cursor = Cursors.Cross;
            _connection.StrokeDashArray = new DoubleCollection(new double[] { 1, 2 });

            if (sender == _sourceDragThumb)
            {
                _fixConnector = _connection.Sink;
                _dragConnector = _connection.Source;
            }
            else if (sender == _sinkDragThumb)
            {
                _dragConnector = _connection.Sink;
                _fixConnector = _connection.Source;
            }
        }

        private void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            Point currentPosition = Mouse.GetPosition(this);
            HitTesting(currentPosition);
            _pathGeometry = UpdatePathGeometry(currentPosition);
            InvalidateVisual();
        }

        private void AnchorPositionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("AnchorPositionSource"))
            {
                Canvas.SetLeft(_sourceDragThumb, _connection.AnchorPositionSource.X);
                Canvas.SetTop(_sourceDragThumb, _connection.AnchorPositionSource.Y);
            }

            if (e.PropertyName.Equals("AnchorPositionSink"))
            {
                Canvas.SetLeft(_sinkDragThumb, _connection.AnchorPositionSink.X);
                Canvas.SetTop(_sinkDragThumb, _connection.AnchorPositionSink.Y);
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawGeometry(null, _drawingPen, _pathGeometry);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _adornerCanvas.Arrange(new Rect(0, 0, _designerCanvas.ActualWidth, _designerCanvas.ActualHeight));
            return finalSize;
        }
        #endregion

        #region Methods
        protected override Visual GetVisualChild(int index)
        {
            return _visualChildren[index];
        }

        private void InitializeDragThumbs()
        {
            Style dragThumbStyle = _connection.FindResource("ConnectionAdornerThumbStyle") as Style;

            // source drag thumb
            _sourceDragThumb = new Thumb();
            Canvas.SetLeft(_sourceDragThumb, _connection.AnchorPositionSource.X);
            Canvas.SetTop(_sourceDragThumb, _connection.AnchorPositionSource.Y);
            _adornerCanvas.Children.Add(_sourceDragThumb);
            if (dragThumbStyle != null)
                _sourceDragThumb.Style = dragThumbStyle;

            _sourceDragThumb.DragDelta += OnThumbDragDelta;
            _sourceDragThumb.DragStarted += OnThumbDragStarted;
            _sourceDragThumb.DragCompleted += OnThumbDragCompleted;

            // sink drag thumb
            _sinkDragThumb = new Thumb();
            Canvas.SetLeft(_sinkDragThumb, _connection.AnchorPositionSink.X);
            Canvas.SetTop(_sinkDragThumb, _connection.AnchorPositionSink.Y);
            _adornerCanvas.Children.Add(_sinkDragThumb);
            if (dragThumbStyle != null)
                _sinkDragThumb.Style = dragThumbStyle;

            _sinkDragThumb.DragDelta += OnThumbDragDelta;
            _sinkDragThumb.DragStarted += OnThumbDragStarted;
            _sinkDragThumb.DragCompleted += OnThumbDragCompleted;
        }

        private PathGeometry UpdatePathGeometry(Point position)
        {
            PathGeometry geometry = new PathGeometry();

            ConnectorOrientation targetOrientation;
            if (HitConnector != null)
                targetOrientation = HitConnector.Orientation;
            else
                targetOrientation = _dragConnector.Orientation;

            List<Point> linePoints = PathFinder.GetConnectionLine(_fixConnector.GetInfo(), position, targetOrientation);

            if (linePoints.Count > 0)
            {
                PathFigure figure = new PathFigure();
                figure.StartPoint = linePoints[0];
                linePoints.Remove(linePoints[0]);
                figure.Segments.Add(new PolyLineSegment(linePoints, true));
                geometry.Figures.Add(figure);
            }

            return geometry;
        }

        private void HitTesting(Point hitPoint)
        {
            bool hitConnectorFlag = false;

            DependencyObject hitObject = _designerCanvas.InputHitTest(hitPoint) as DependencyObject;
            while (hitObject != null &&
                   hitObject != _fixConnector.ParentDesignerItem &&
                   hitObject.GetType() != typeof(DesignerCanvas))
            {
                if (hitObject is Connector)
                {
                    HitConnector = hitObject as Connector;
                    hitConnectorFlag = true;
                }

                if (hitObject is DesignerItem)
                {
                    HitDesignerItem = hitObject as DesignerItem;
                    if (!hitConnectorFlag)
                        HitConnector = null;
                    return;
                }
                hitObject = VisualTreeHelper.GetParent(hitObject);
            }

            HitConnector = null;
            HitDesignerItem = null;
        }
        #endregion
    }
}
