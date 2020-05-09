using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.Models
{
    public class Graph
    {
        public List<GraphNode> Nodes { get; set; } = new List<GraphNode>();
        public List<GraphLink> Links { get; set; } = new List<GraphLink>();
    }

    public class GraphNode
    {
        public string key { get; set; }
        public string group { get; set; }
        public string text { get; set; }
        public bool isGroup { get; set; } = false;
    }

    public class GraphLink
    {
        public string from { get; set; }
        public string to { get; set; }
        public string connectionId { get; set; }
        public string color
        {
            get
            {
                var hue = 0;
                if (connectionId != null)
                    hue = Math.Abs(connectionId.GetHashCode()) % 240;
                return new AliaaCommon.HSLColor(hue, 240.0, 120.0).ToHexString();
            }
        }
    }
}
