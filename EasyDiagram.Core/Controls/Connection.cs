using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyDiagram.Core
{
    public class Connection : Control, ISelectable, INotifyPropertyChanged
    {
        #region Constructors
        public Connection(Connector source, Connector sink)
        {
            Id = Guid.NewGuid();
            Source = source;
            Sink = sink;
            Unloaded += OnUnloaded;
        }
        #endregion

        #region Properties
        public Guid Id { get; set; }

        /// <summary>
        /// Source connector
        /// </summary>
        public virtual Connector Source
        {
            get => _source;
            set
            {
                if (_source != value)
                {
                    if (_source != null)
                    {
                        _source.PropertyChanged -= OnConnectorPositionChanged;
                        _source.Connections.Remove(this);
                    }

                    _source = value;

                    if (_source != null)
                    {
                        _source.Connections.Add(this);
                        _source.PropertyChanged += OnConnectorPositionChanged;
                    }

                    UpdatePathGeometry();
                }
            }
        }

        /// <summary>
        /// Sink connector
        /// </summary>
        public virtual Connector Sink
        {
            get => _sink;
            set
            {
                if (_sink != value)
                {

                    if (_sink != null)
                    {
                        _sink.PropertyChanged -= OnConnectorPositionChanged;
                        _sink.Connections.Remove(this);
                    }

                    _sink = value;

                    if (_sink != null)
                    {
                        _sink.Connections.Add(this);
                        _sink.PropertyChanged += OnConnectorPositionChanged;
                    }

                    UpdatePathGeometry();
                }
            }
        }

        /// <summary>
        /// Connection path geomatry
        /// </summary>
        public virtual PathGeometry PathGeometry
        {
            get => _pathGeomatry;
            set
            {
                if (_pathGeomatry != value)
                {
                    _pathGeomatry = value;
                    UpdateAnchorPosition();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Between source connector position and the beginning 
        /// of the path geometry we leave some space for visual reasons.
        /// So the anchor position source really marks the beginning 
        /// of the path geometry on the source side
        /// </summary>
        public virtual Point AnchorPositionSource
        {
            get => _anchorPositionSource;
            set
            {
                if (_anchorPositionSource != value)
                {
                    _anchorPositionSource = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Slope of the path at the anchor position
        /// needed for the rotation angle of the arrow
        /// </summary>
        public virtual double AnchorAngleSource
        {
            get => _anchorAngleSource;
            set
            {
                if (_anchorAngleSource != value)
                {
                    _anchorAngleSource = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Analogue to source side
        /// </summary>
        public virtual Point AnchorPositionSink
        {
            get => _anchorPositionSink;
            set
            {
                if (_anchorPositionSink != value)
                {
                    _anchorPositionSink = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// analogue to source side
        /// </summary>
        public virtual double AnchorAngleSink
        {
            get => _anchorAngleSink;
            set
            {
                if (_anchorAngleSink != value)
                {
                    _anchorAngleSink = value;
                    OnPropertyChanged();
                }
            }
        }

        public virtual ArrowSymbol SourceArrowSymbol
        {
            get => _sourceArrowSymbol;
            set
            {
                if (_sourceArrowSymbol != value)
                {
                    _sourceArrowSymbol = value;
                    OnPropertyChanged();
                }
            }
        }

        public virtual ArrowSymbol SinkArrowSymbol
        {
            get => _sinkArrowSymbol;
            set
            {
                if (_sinkArrowSymbol != value)
                {
                    _sinkArrowSymbol = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Specifies a point at half path length
        /// </summary>
        public Point LabelPosition
        {
            get => _labelPosition;
            set
            {
                if (_labelPosition != value)
                {
                    _labelPosition = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Pattern of dashes and gaps that is used to outline the connection path
        /// </summary>
        public virtual DoubleCollection StrokeDashArray
        {
            get => _strokeDashArray;
            set
            {
                if (_strokeDashArray != value)
                {
                    _strokeDashArray = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// If selected, the ConnectionAdorner becomes visible
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();

                    if (_isSelected)
                    {
                        ShowAdorner();
                    }
                    else
                    {
                        HideAdorner();
                    }
                }
            }
        }
        #endregion

        #region Members
        Connector _source;
        Connector _sink;
        Adorner _connectionAdorner;
        PathGeometry _pathGeomatry;
        Point _anchorPositionSource;
        double _anchorAngleSource;
        Point _anchorPositionSink;
        double _anchorAngleSink;
        ArrowSymbol _sourceArrowSymbol;
        ArrowSymbol _sinkArrowSymbol;
        Point _labelPosition;
        DoubleCollection _strokeDashArray;
        bool _isSelected;
        #endregion

        #region Event handlers
        private void OnConnectorPositionChanged(object sender, PropertyChangedEventArgs e)
        {
            // Whenever the 'Position' property of the source or sink Connector 
            // changes we must update the connection path geometry
            if (e.PropertyName.Equals("Position"))
            {
                UpdatePathGeometry();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Remove event handlers
            Source = null;
            Sink = null;

            // Remove andorner
            if (_connectionAdorner != null)
            {
                DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(_connectionAdorner);
                    _connectionAdorner = null;
                }
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            // Usual selection business
            DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;
            if (designer != null)
            {
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                {
                    if (IsSelected)
                    {
                        designer.SelectionService.RemoveFromSelection(this);
                    }
                    else
                    {
                        designer.SelectionService.AddToSelection(this);
                    }
                }
                else if (!IsSelected)
                {
                    designer.SelectionService.SelectItem(this);
                }

                Focus();
            }
            e.Handled = false;
        }
        #endregion

        #region Methods
        internal virtual void ShowAdorner()
        {
            if (_connectionAdorner == null)
            {
                DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    _connectionAdorner = new ConnectionAdorner(designer, this);
                    adornerLayer.Add(_connectionAdorner);
                }
            }
            _connectionAdorner.Visibility = Visibility.Visible;
        }

        internal virtual void HideAdorner()
        {
            if (_connectionAdorner != null)
                _connectionAdorner.Visibility = Visibility.Collapsed;
        }

        protected virtual void UpdateAnchorPosition()
        {
            Point pathStartPoint, pathTangentAtStartPoint;
            Point pathEndPoint, pathTangentAtEndPoint;
            Point pathMidPoint, pathTangentAtMidPoint;

            // the PathGeometry.GetPointAtFractionLength method gets the point and a tangent vector 
            // on PathGeometry at the specified fraction of its length
            this.PathGeometry.GetPointAtFractionLength(0, out pathStartPoint, out pathTangentAtStartPoint);
            this.PathGeometry.GetPointAtFractionLength(1, out pathEndPoint, out pathTangentAtEndPoint);
            this.PathGeometry.GetPointAtFractionLength(0.5, out pathMidPoint, out pathTangentAtMidPoint);

            // get angle from tangent vector
            this.AnchorAngleSource = Math.Atan2(-pathTangentAtStartPoint.Y, -pathTangentAtStartPoint.X) * (180 / Math.PI);
            this.AnchorAngleSink = Math.Atan2(pathTangentAtEndPoint.Y, pathTangentAtEndPoint.X) * (180 / Math.PI);

            // add some margin on source and sink side for visual reasons only
            pathStartPoint.Offset(-pathTangentAtStartPoint.X * 5, -pathTangentAtStartPoint.Y * 5);
            pathEndPoint.Offset(pathTangentAtEndPoint.X * 5, pathTangentAtEndPoint.Y * 5);

            this.AnchorPositionSource = pathStartPoint;
            this.AnchorPositionSink = pathEndPoint;
            this.LabelPosition = pathMidPoint;
        }

        protected virtual void UpdatePathGeometry()
        {
            if (Source != null && Sink != null)
            {
                PathGeometry geometry = new PathGeometry();
                List<Point> linePoints = PathFinder.GetConnectionLine(Source.GetInfo(), Sink.GetInfo(), true);
                if (linePoints.Count > 0)
                {
                    PathFigure figure = new PathFigure();
                    figure.StartPoint = linePoints[0];
                    linePoints.Remove(linePoints[0]);
                    figure.Segments.Add(new PolyLineSegment(linePoints, true));
                    geometry.Figures.Add(figure);

                    PathGeometry = geometry;
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public virtual event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
