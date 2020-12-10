using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;

namespace EasyDiagram.Core
{
    public class DragThumb : Thumb
    {
        #region Constructors
        public DragThumb()
        {
            DragDelta += OnDragDelta;
        }
        #endregion

        #region Event handlers
        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            // Need implement
        }
        #endregion
    }
}
