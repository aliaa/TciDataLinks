using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyMongoNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using TciCommon.Models;
using TciDataLinks.Models;

namespace TciDataLinks.Controllers
{
    [Authorize]
    public class GraphController : Controller
    {
        private readonly IDbContext db;

        public GraphController(IDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Center(ObjectId id)
        {
            var graph = new Graph();
            var center = db.FindById<CommCenter>(id);
            var centerNode = new GraphNode
            {
                key = "Center_" + center.Id,
                text = "مرکز " + center.Name,
                isGroup = true
            };
            graph.Nodes.Add(centerNode);
            var deviceIds = new List<ObjectId>();
            foreach (var building in db.FindGetResults<Building>(b => b.Parent == id))
            {
                var buildingNode = new GraphNode
                {
                    key = "Building_" + building.Id,
                    text = "ساختمان " + building.Name,
                    group = centerNode.key,
                    isGroup = true
                };
                graph.Nodes.Add(buildingNode);
                foreach (var room in db.FindGetResults<Room>(r => r.Parent == building.Id))
                {
                    var roomNode = new GraphNode
                    {
                        key = "Room_" + room.Id,
                        text = "سالن " + room.Name,
                        group = buildingNode.key,
                        isGroup = true
                    };
                    graph.Nodes.Add(roomNode);
                    foreach (var rack in db.FindGetResults<Rack>(r => r.Parent == room.Id))
                    {
                        var rackNode = new GraphNode
                        {
                            key = "Rack_" + rack.Id,
                            text = "راک " + rack.ToString(),
                            group = roomNode.key,
                            isGroup = true
                        };
                        graph.Nodes.Add(rackNode);
                        foreach (var device in db.FindGetResults<Device>(d => d.Rack == rack.Id))
                        {
                            deviceIds.Add(device.Id);
                            var deviceNode = new GraphNode
                            {
                                key = "Device_" + device.Id,
                                text = device.ToString(),
                                group = rackNode.key
                            };
                            graph.Nodes.Add(deviceNode);
                        }
                        foreach (var pp in db.FindGetResults<PatchPanel>(p => p.Rack == rack.Id))
                        {
                            var ppNode = new GraphNode
                            {
                                key = "PatchPanel_" + pp.Id,
                                text = pp.ToString(),
                                group = rackNode.key
                            };
                            graph.Nodes.Add(ppNode);
                        }
                    }
                }
            }

            var connections = db.FindGetResults<EndPoint>(e => deviceIds.Contains(e.Device)).GroupBy(key => key.Connection);
            foreach (var c in connections)
            {
                var endPoints = db.Find<EndPoint>(e => e.Connection == c.Key).SortBy(e => e.Index).ToList();
                string lastKey = null;
                for (int i = 0; i < endPoints.Count; i++)
                {
                    var ep = endPoints[i];
                    var device = db.FindById<Device>(ep.Device);
                    var deviceKey = "Device_" + ep.Device;
                    var anotherCenter = CheckDeviceParents(graph, device, centerNode.key);
                    if (anotherCenter != null)
                    {
                        if (lastKey != anotherCenter)
                        {
                            graph.Links.Add(new GraphLink { from = lastKey, to = anotherCenter, connectionId = c.Key.ToString() });
                            lastKey = anotherCenter;
                        }
                        continue;
                    }
                    if (lastKey != null)
                        graph.Links.Add(new GraphLink { from = lastKey, to = deviceKey, connectionId = c.Key.ToString() });
                    lastKey = deviceKey;
                    foreach (var pc in ep.PassiveConnections)
                    {
                        var ppKey = "PatchPanel_" + pc.PatchPanel;
                        if (!graph.Nodes.Any(n => n.key == ppKey))
                        {
                            var pp = db.FindById<PatchPanel>(pc.PatchPanel);
                            graph.Nodes.Add(new GraphNode { key = ppKey, text = pp.ToString(), group = "Rack_" + device.Rack });
                        }
                        graph.Links.Add(new GraphLink { from = lastKey, to = ppKey, connectionId = c.Key.ToString() });
                        lastKey = ppKey;
                    }
                }
            }

            return Json(graph, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private string CheckDeviceParents(Graph graph, Device device, string restrictCenterKey = null)
        {
            var deviceKey = "Device_" + device.Id;
            if (graph.Nodes.Any(n => n.key == deviceKey))
                return null;
            var rack = db.FindById<Rack>(device.Rack);
            var rackKey = "Rack_" + rack.Id;
            var room = db.FindById<Room>(rack.Parent);
            var roomKey = "Room_" + room.Id;
            var building = db.FindById<Building>(room.Parent);
            var buildingKey = "Building_" + building.Id;
            var center = db.FindById<CommCenter>(building.Parent);
            var centerKey = "Center_" + center.Id;
            if (!graph.Nodes.Any(n => n.key == centerKey))
                graph.Nodes.Add(new GraphNode { key = centerKey, text = "مرکز " + center.Name, isGroup = true });
            if (restrictCenterKey != null && centerKey != restrictCenterKey)
                return centerKey;
            if (!graph.Nodes.Any(n => n.key == buildingKey))
                graph.Nodes.Add(new GraphNode { key = buildingKey, text = "ساختمان " + building.Name, group = centerKey, isGroup = true });
            if (!graph.Nodes.Any(n => n.key == roomKey))
                graph.Nodes.Add(new GraphNode { key = roomKey, text = "سالن " + room.Name, group = buildingKey, isGroup = true });
            if (!graph.Nodes.Any(n => n.key == rackKey))
                graph.Nodes.Add(new GraphNode { key = rackKey, text = "راک " + rack.ToString(), group = roomKey, isGroup = true });
            graph.Nodes.Add(new GraphNode { key = deviceKey, text = device.ToString(), group = rackKey });
            return null;
        }

        public IActionResult Connection(ObjectId id)
        {
            var graph = new Graph();
            var endPoints = db.Find<EndPoint>(e => e.Connection == id).SortBy(e => e.Index).ToList();
            string lastKey = null;
            foreach (var ep in endPoints)
            {
                var device = db.FindById<Device>(ep.Device);
                CheckDeviceParents(graph, device);
                var deviceKey = "Device_" + ep.Device;
                if (lastKey != null)
                    graph.Links.Add(new GraphLink { from = lastKey, to = deviceKey, connectionId = id.ToString() });
                lastKey = deviceKey;
                foreach (var pc in ep.PassiveConnections)
                {
                    var ppKey = "PatchPanel_" + pc.PatchPanel;
                    if (!graph.Nodes.Any(n => n.key == ppKey))
                    {
                        var pp = db.FindById<PatchPanel>(pc.PatchPanel);
                        graph.Nodes.Add(new GraphNode { key = ppKey, text = pp.ToString(), group = "Rack_" + device.Rack });
                    }
                    graph.Links.Add(new GraphLink { from = lastKey, to = ppKey, connectionId = id.ToString() });
                    lastKey = ppKey;
                }
            }

            return Json(graph, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}