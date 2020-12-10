using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace EasyDiagram.Core
{
    /// <summary>
    /// Provides compact info about a connector; used for the 
    /// routing algorithm, instead of hand over a full fledged Connector
    /// </summary>
    public struct ConnectorInfo
    {
        public double DesignerItemLeft { get; set; }
        public double DesignerItemTop { get; set; }
        public Size DesignerItemSize { get; set; }
        public Point Position { get; set; }
        public ConnectorOrientation Orientation { get; set; }
    }
}
