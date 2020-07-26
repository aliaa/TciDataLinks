using EasyMongoNet;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace TciDataLinks.Models
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

        public void SetLocations(Dictionary<string, NodeLocation> locations)
        {
            foreach (var key in locations.Keys)
            {
                if (nodesDic.ContainsKey(key))
                    nodesDic[key].loc = locations[key];
            }
        }
    }

    public class GraphNode
    {
        public string key { get; set; }
        public string group { get; set; }
        public string text { get; set; }
        public bool isGroup { get; set; } = false;
        public NodeLocation loc { get; set; }
        public string image { get; set; }
    }

    public class NodeLocation
    {
        public float x { get; set; }
        public float y { get; set; }
        public bool s { get; set; }
    }

    [CollectionIndex(new string[] { nameof(Center), nameof(Key) }, Unique = true)]
    public class NodeLocationWithKey : MongoEntity
    {
        public ObjectId Center { get; set; }
        public string Key { get; set; }
        public NodeLocation Loc { get; set; }
    }

    public class GraphLink
    {
        public GraphLink() { }

        public GraphLink(string from, string to, ObjectId id, string text)
        {
            this.from = from;
            this.to = to;
            this.connectionId = id.ToString();
            this.text = text;
        }

        public GraphLink(string from, string to, Connection connection, string fromPort, string toPort)
            : this(from, to, connection.Id, "Link: " +  connection.IdInt + "\nPort: " + fromPort + " ==> " + toPort) { }

        public string from { get; set; }
        public string to { get; set; }
        public string connectionId { get; set; }
        public string text { get; set; }
        public string color
        {
            get
            {
                var hue = 0;
                if (connectionId != null)
                    hue = Math.Abs(connectionId.GetHashCode()) % 240;
                return new AliaaCommon.HSLColor(hue, 220.0, 90.0, 150.0).ToHexString();
            }
        }
    }
}
