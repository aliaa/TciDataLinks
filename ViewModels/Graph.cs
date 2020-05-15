using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace TciDataLinks.ViewModels
{
    public class Graph
    {
        private readonly SortedDictionary<string, GraphNode> nodesDic = new SortedDictionary<string, GraphNode>();
        private readonly List<GraphLink> links = new List<GraphLink>();

        public bool AddNode(GraphNode node)
        {
            if (nodesDic.ContainsKey(node.key))
                return false;
            nodesDic.Add(node.key, node);
            return true;
        }

        public bool RemoveNode(string key) => nodesDic.Remove(key);
        public bool ContainsNodeKey(string key) => nodesDic.ContainsKey(key);
        public IEnumerable<GraphNode> Nodes => nodesDic.Values;

        public void AddLink(GraphLink link)
        {
            links.Add(link);
        }

        public IEnumerable<GraphLink> Links => links;
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
        public GraphLink() { }

        public GraphLink(string from, string to, ObjectId id)
        {
            this.from = from;
            this.to = to;
            this.connectionId = id.ToString();
        }

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
