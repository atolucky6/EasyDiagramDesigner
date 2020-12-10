using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyDiagram.Core
{
    public class ConnectorAdorner : Adorner
    {
        #region Constructors
        public ConnectorAdorner(DesignerCanvas designerCanvas, Connector sourceConnector) : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _sourceConnector = sourceConnector;
            _drawingPen = new Pen(Brushes.LightSlateGray, 1);
            _drawingPen.LineJoin = PenLineJoin.Round;
            Cursor = Cursors.Cross;
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
        #endregion

        #region Members
        PathGeometry _pathGeometry;
        DesignerCanvas _designerCanvas;
        Connector _sourceConnector;
        Pen _drawingPen;
        DesignerItem _hitDesignerItem;
        Connector _hitConnector;
        #endregion

        #region Event handlers
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (HitConnector != null)
            {
                Connector sourceConnector = _sourceConnector;
                Connector sinkConnector = this.HitConnector;
                Connection newConnection = new Connection(sourceConnector, sinkConnector);

                Canvas.SetZIndex(newConnection, _designerCanvas.Children.Count);
                _designerCanvas.Children.Add(newConnection);

            }
            if (HitDesignerItem != null)
            {
                this.HitDesignerItem.IsDragConnectionOver = false;
            }

            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
            if (adornerLayer != null)
            {
                adornerLayer.Remove(this);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured) this.CaptureMouse();
                HitTesting(e.GetPosition(this));
                _pathGeometry = GetPathGeometry(e.GetPosition(this));
                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawGeometry(null, _drawingPen, _pathGeometry);

            // without a background the OnMouseMove event would not be fired
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        }
        #endregion

        #region Methods
        private PathGeometry GetPathGeometry(Point position)
        {
            PathGeometry geometry = new PathGeometry();

            ConnectorOrientation targetOrientation;
            if (HitConnector != null)
                targetOrientation = HitConnector.Orientation;
            else
                targetOrientation = ConnectorOrientation.None;

            List<Point> pathPoints = PathFinder.GetConnectionLine(_sourceConnector.GetInfo(), position, targetOrientation);

            if (pathPoints.Count > 0)
            {
                PathFigure figure = new PathFigure();
                figure.StartPoint = pathPoints[0];
                pathPoints.Remove(pathPoints[0]);
                figure.Segments.Add(new PolyLineSegment(pathPoints, true));
                geometry.Figures.Add(figure);
            }

            return geometry;
        }

        private void HitTesting(Point hitPoint)
        {
            bool hitConnectorFlag = false;

            DependencyObject hitObject = _designerCanvas.InputHitTest(hitPoint) as DependencyObject;
            while (hitObject != null &&
                   hitObject != _sourceConnector.ParentDesignerItem &&
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
