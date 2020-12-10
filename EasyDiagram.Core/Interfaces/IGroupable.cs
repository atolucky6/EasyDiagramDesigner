using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDiagram.Core
{
    public interface IGroupable
    {
        Guid Id { get; }
        Guid ParentId { get; set; }
        bool IsGroup { get; set; }
    }
}
