using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDiagram.Core
{
    /// <summary>
    /// Define that object can be select on designer region
    /// </summary>
    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }
}
