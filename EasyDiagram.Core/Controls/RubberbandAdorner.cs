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
    public class RubberbandAdorner : Adorner
    {
        #region Constructors
        public RubberbandAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint) : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            _rubberbandPen = new Pen(Brushes.LightSlateGray, 1);
            _rubberbandPen.DashStyle = new DashStyle(new double[] { 2 }, 1);
        }
        #endregion

        #region Members
        Point? _startPoint;
        Point? _endPoint;
        Pen _rubberbandPen;
        DesignerCanvas _designerCanvas;
        #endregion

        #region Event handlers
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!IsMouseCaptured)
                    CaptureMouse();

                _endPoint = e.GetPosition(this);
                UpdateSelection();
                InvalidateVisual();
            }
            else
            {
                if (IsMouseCaptured) 
                    ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            // release mouse capture
            if (IsMouseCaptured) 
                ReleaseMouseCapture();

            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
            if (adornerLayer != null)
                adornerLayer.Remove(this);

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // without a background the OnMouseMove event would not be fired!
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (_startPoint.HasValue && _endPoint.HasValue)
                dc.DrawRectangle(Brushes.Transparent, _rubberbandPen, new Rect(_startPoint.Value, _endPoint.Value));
        }
        #endregion

        #region Methods
        private void UpdateSelection()
        {
            _designerCanvas.SelectionService.ClearSelection();

            Rect rubberband = new Rect(_startPoint.Value, _endPoint.Value);
            foreach (Control item in _designerCanvas.Children)
            {
                Rect itemRect = VisualTreeHelper.GetDescendantBounds(item);
                Rect itemBounds = item.TransformToAncestor(_designerCanvas).TransformBounds(itemRect);

                if (rubberband.Contains(itemBounds))
                {
                    if (item is Connection)
                    {
                        _designerCanvas.SelectionService.AddToSelection(item as ISelectable);
                    }
                    else
                    {
                        DesignerItem designerItem = item as DesignerItem;
                        if (designerItem.ParentId == Guid.Empty)
                            _designerCanvas.SelectionService.AddToSelection(designerItem);
                    }
                }
            }
        }
        #endregion
    }
}
