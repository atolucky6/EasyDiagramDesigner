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
    public class Connector : Control, INotifyPropertyChanged
    {
        #region Constructors
        public Connector()
        {
            LayoutUpdated += OnLayoutUpdated;
        }
        #endregion

        #region Public properties
        public ConnectorOrientation Orientation { get; set; }

        /// <summary>
        /// Connections that link to this connector
        /// </summary>
        public virtual List<Connection> Connections
        {
            get
            {
                if (_connections == null)
                    _connections = new List<Connection>();
                return _connections;
            }
        }

        /// <summary>
        /// The DesignerItem this Connector belongs to;
        /// retrieved from DataContext, which is set in the
        /// DesignerItem template
        /// </summary>
        public virtual DesignerItem ParentDesignerItem
        {
            get
            {
                if (_parentDesignerItem == null)
                    _parentDesignerItem = this.DataContext as DesignerItem;
                return _parentDesignerItem;
            }
        }

        /// <summary>
        /// Center position of this Connector relative to the DesignerCanvas
        /// </summary>
        public virtual Point Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Members
        Point? _dragStartPoint = null;
        List<Connection> _connections;
        DesignerItem _parentDesignerItem;
        Point _position;
        #endregion

        #region Event handlers
        protected void OnLayoutUpdated(object sender, EventArgs e)
        {
            // When the layout changes we update the position property
            DesignerCanvas designer = GetDesignerCanvas(this);
            if (designer != null)
            {
                //get center position of this Connector relative to the DesignerCanvas
                this.Position = this.TransformToAncestor(designer).Transform(new Point(this.Width / 2, this.Height / 2));
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DesignerCanvas canvas = GetDesignerCanvas(this);
            if (canvas != null)
            {
                // position relative to DesignerCanvas
                _dragStartPoint = new Point?(e.GetPosition(canvas));
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button is not pressed we have no drag operation, ...
            if (e.LeftButton != MouseButtonState.Pressed)
                _dragStartPoint = null;

            // but if mouse button is pressed and start point value is set we do have one
            if (_dragStartPoint.HasValue)
            {
                // create connection adorner 
                DesignerCanvas canvas = GetDesignerCanvas(this);
                if (canvas != null)
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                    if (adornerLayer != null)
                    {
                        ConnectorAdorner adorner = new ConnectorAdorner(canvas, this);
                        if (adorner != null)
                        {
                            adornerLayer.Add(adorner);
                            e.Handled = true;
                        }
                    }
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Iterate through visual tree to get parent DesignerCanvas
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private DesignerCanvas GetDesignerCanvas(DependencyObject element)
        {
            while (element != null && !(element is DesignerCanvas))
                element = VisualTreeHelper.GetParent(element);

            return element as DesignerCanvas;
        }

        public ConnectorInfo GetInfo()
        {
            ConnectorInfo info = new ConnectorInfo
            {
                DesignerItemLeft = DesignerCanvas.GetLeft(this.ParentDesignerItem),
                DesignerItemTop = DesignerCanvas.GetTop(this.ParentDesignerItem),
                DesignerItemSize = new Size(this.ParentDesignerItem.ActualWidth, this.ParentDesignerItem.ActualHeight),
                Orientation = this.Orientation,
                Position = this.Position
            };
            return info;
        }
        #endregion

        #region INotifyPropertyChanged
        public virtual event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
