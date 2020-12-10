using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyDiagram.Core
{
    //These attributes identify the types of the named parts that are used for templating
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]
    [TemplatePart(Name = "PART_ResizeDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ConnectorDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    public class DesignerItem : ContentControl, ISelectable, IGroupable
    {
        #region Constructors
        static DesignerItem()
        {
            // Set the key to reference the style for this control
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DesignerItem), new FrameworkPropertyMetadata(typeof(DesignerItem)));
        }

        public DesignerItem(Guid id)
        {
            Id = id;
            Loaded += OnLoaded;
        }

        public DesignerItem() : this(Guid.NewGuid()) { }
        #endregion

        #region Properties
        public Guid Id { get; protected set; }
        #endregion

        #region Dependency properties

        #region ParentId
        public Guid ParentId
        {
            get { return (Guid)GetValue(ParentIdProperty); }
            set { SetValue(ParentIdProperty, value); }
        }
        public static readonly DependencyProperty ParentIdProperty = DependencyProperty.Register("ParentId", typeof(Guid), typeof(DesignerItem));
        #endregion

        #region IsGroup
        public bool IsGroup
        {
            get { return (bool)GetValue(IsGroupProperty); }
            set { SetValue(IsGroupProperty, value); }
        }
        public static readonly DependencyProperty IsGroupProperty = DependencyProperty.Register("IsGroup", typeof(bool), typeof(DesignerItem));
        #endregion

        #region IsSelected
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(false));
        #endregion

        #region DragThumbTemplate
        public static readonly DependencyProperty DragThumbTemplateProperty =
            DependencyProperty.RegisterAttached("DragThumbTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetDragThumbTemplate(UIElement element)
        {
            return (ControlTemplate)element.GetValue(DragThumbTemplateProperty);
        }

        public static void SetDragThumbTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(DragThumbTemplateProperty, value);
        }
        #endregion

        #region ConnectorDecoratorTemplate
        public static readonly DependencyProperty ConnectorDecoratorTemplateProperty =
            DependencyProperty.RegisterAttached("ConnectorDecoratorTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetConnectorDecoratorTemplate(UIElement element)
        {
            return (ControlTemplate)element.GetValue(ConnectorDecoratorTemplateProperty);
        }

        public static void SetConnectorDecoratorTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(ConnectorDecoratorTemplateProperty, value);
        }
        #endregion

        #region IsDragConnectionOver
        /// <summary>
        /// While drag connection procedure is ongoing and the mouse moves over 
        /// this item this value is true; if true the ConnectorDecorator is triggered
        /// to be visible, see template
        /// </summary>
        public bool IsDragConnectionOver
        {
            get { return (bool)GetValue(IsDragConnectionOverProperty); }
            set { SetValue(IsDragConnectionOverProperty, value); }
        }
        public static readonly DependencyProperty IsDragConnectionOverProperty =
            DependencyProperty.Register("IsDragConnectionOver",
                                         typeof(bool),
                                         typeof(DesignerItem),
                                         new FrameworkPropertyMetadata(false));
        #endregion



        #endregion

        #region Event handlers
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (base.Template != null)
            {
                ContentPresenter contentPresenter =
                    this.Template.FindName("PART_ContentPresenter", this) as ContentPresenter;
                if (contentPresenter != null)
                {
                    UIElement contentVisual = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;
                    if (contentVisual != null)
                    {
                        DragThumb thumb = this.Template.FindName("PART_DragThumb", this) as DragThumb;
                        if (thumb != null)
                        {
                            ControlTemplate template =
                                DesignerItem.GetDragThumbTemplate(contentVisual) as ControlTemplate;
                            if (template != null)
                                thumb.Template = template;
                        }
                    }
                }
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

            // update selection
            if (designer != null)
            {
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                    if (IsSelected)
                    {
                        designer.SelectionService.RemoveFromSelection(this);
                    }
                    else
                    {
                        designer.SelectionService.AddToSelection(this);
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
    }
}
