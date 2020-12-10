using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace EasyDiagram.Core
{
    /// <summary>
    /// Wraps info of the dragged object into a class
    /// </summary>
    public class DragObject
    {
        /// <summary>
        /// Xaml string that represents the serialized content
        /// </summary>
        public string Xaml { get; set; }

        /// <summary>
        /// Defines width and height of the DesignerItem
        /// when this DragObject is dropped on the DesignerCanvas
        /// </summary>
        public Size? DesiredSize { get; set; } 
    }
}
