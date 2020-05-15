﻿using System;
using System.Collections.Generic;
using System.Linq;
using EasyMongoNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using TciCommon.Models;
using TciDataLinks.Models;
using TciDataLinks.ViewModels;

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
            graph.AddNode(centerNode);
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
                graph.AddNode(buildingNode);
                foreach (var room in db.FindGetResults<Room>(r => r.Parent == building.Id))
                {
                    var roomNode = new GraphNode
                    {
                        key = "Room_" + room.Id,
                        text = "سالن " + room.Name,
                        group = buildingNode.key,
                        isGroup = true
                    };
                    graph.AddNode(roomNode);
                    foreach (var rack in db.FindGetResults<Rack>(r => r.Parent == room.Id))
                    {
                        var rackNode = new GraphNode
                        {
                            key = "Rack_" + rack.Id,
                            text = "راک " + rack.ToString(),
                            group = roomNode.key,
                            isGroup = true
                        };
                        graph.AddNode(rackNode);
                        foreach (var device in db.FindGetResults<Device>(d => d.Rack == rack.Id))
                        {
                            deviceIds.Add(device.Id);
                            var deviceNode = new GraphNode
                            {
                                key = "Device_" + device.Id,
                                text = device.ToString(),
                                group = rackNode.key
                            };
                            graph.AddNode(deviceNode);
                        }
                        foreach (var pp in db.FindGetResults<PatchPanel>(p => p.Rack == rack.Id))
                        {
                            var ppNode = new GraphNode
                            {
                                key = "PatchPanel_" + pp.Id,
                                text = pp.ToString(),
                                group = rackNode.key
                            };
                            graph.AddNode(ppNode);
                        }
                    }
                }
            }

            var connections = db.FindGetResults<EndPoint>(e => deviceIds.Contains(e.Device)).GroupBy(key => key.Connection);
            foreach (var c in connections)
            {
                var endPoints = db.Find<EndPoint>(e => e.Connection == c.Key).SortBy(e => e.Index).ToList();
                string lastKey = null;

                var device = db.FindById<Device>(endPoints[0].Device);
                AddDeviceHierarchal(graph, device, out string deviceKey, out string anotherCenter, centerNode.key);
                lastKey = deviceKey;
                if (anotherCenter == null)
                {
                    foreach (var pc in endPoints[0].PassiveConnections)
                    {
                        AddDeviceHierarchal(graph, db.FindById<PatchPanel>(pc.PatchPanel), out string ppKey);
                        graph.AddLink(new GraphLink(lastKey, ppKey, c.Key));
                        lastKey = ppKey;
                    }
                }
                else
                    lastKey = anotherCenter;
                var lastFirstEndPointKey = lastKey;

                for (int i = 1; i < endPoints.Count; i++)
                {
                    var ep = endPoints[i];

                    device = db.FindById<Device>(ep.Device);
                    AddDeviceHierarchal(graph, device, out deviceKey, out anotherCenter, centerNode.key);
                    if (anotherCenter != null)
                    {
                        if (lastKey != anotherCenter)
                        {
                            graph.AddLink(new GraphLink(lastFirstEndPointKey, anotherCenter, c.Key));
                            lastKey = lastFirstEndPointKey;
                        }
                        continue;
                    }

                    for (int j = ep.PassiveConnections.Count - 1; j >= 0; j--)
                    {
                        var pc = ep.PassiveConnections[j];
                        AddDeviceHierarchal(graph, db.FindById<PatchPanel>(pc.PatchPanel), out string ppKey);
                        graph.AddLink(new GraphLink(lastKey, ppKey, c.Key));
                        lastKey = ppKey;
                    }
                    graph.AddLink(new GraphLink(lastKey, deviceKey, c.Key));
                    lastKey = lastFirstEndPointKey;
                }
            }

            return Json(graph, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private void AddDeviceHierarchal(Graph graph, BaseDevice device, out string deviceKey)
        {
            AddDeviceHierarchal(graph, device, out deviceKey, out _);
        }

        private void AddDeviceHierarchal(Graph graph, BaseDevice device, out string deviceKey, out string anotherCenter, string restrictCenterKey = null)
        {
            anotherCenter = null;
            if (device is Device)
                deviceKey = "Device_" + device.Id;
            else if (device is PatchPanel)
                deviceKey = "PatchPanel_" + device.Id;
            else
                throw new NotImplementedException();
            
            if (graph.ContainsNodeKey(deviceKey))
                return;

            var rackKey = "Rack_" + device.Rack;
            graph.AddNode(new GraphNode { key = deviceKey, text = device.ToString(), group = rackKey });
            if (graph.ContainsNodeKey(rackKey))
                return;

            var rack = db.FindById<Rack>(device.Rack);
            var roomKey = "Room_" + rack.Parent;
            graph.AddNode(new GraphNode { key = rackKey, text = "راک " + rack.ToString(), group = roomKey, isGroup = true });
            if (graph.ContainsNodeKey(roomKey))
                return;

            var room = db.FindById<Room>(rack.Parent);
            var buildingKey = "Building_" + room.Parent;
            graph.AddNode(new GraphNode { key = roomKey, text = "سالن " + room.Name, group = buildingKey, isGroup = true });
            if (graph.ContainsNodeKey(buildingKey))
                return;

            var building = db.FindById<Building>(room.Parent);
            var centerKey = "Center_" + building.Parent;
            var center = db.FindById<CommCenter>(building.Parent);
            graph.AddNode(new GraphNode { key = centerKey, text = "مرکز " + center.Name, isGroup = true });
            if (restrictCenterKey != null && centerKey != restrictCenterKey)
            {
                anotherCenter = centerKey;
                graph.RemoveNode(roomKey);
                graph.RemoveNode(rackKey);
                graph.RemoveNode(deviceKey);
                return;
            }

            graph.AddNode(new GraphNode { key = buildingKey, text = "ساختمان " + building.Name, group = centerKey, isGroup = true });
        }

        public IActionResult Connection(ObjectId id)
        {
            var graph = new Graph();
            var endPoints = db.Find<EndPoint>(e => e.Connection == id).SortBy(e => e.Index).ToList();
            string lastKey = null;

            AddDeviceHierarchal(graph, db.FindById<Device>(endPoints[0].Device), out lastKey);
            foreach (var pc in endPoints[0].PassiveConnections)
            {
                AddDeviceHierarchal(graph, db.FindById<PatchPanel>(pc.PatchPanel), out string ppKey);
                graph.AddLink(new GraphLink(lastKey, ppKey, id));
                lastKey = ppKey;
            }
            var lastFirstEndPointKey = lastKey;

            for (int i = 1; i < endPoints.Count; i++)
            {
                for (int j = endPoints[i].PassiveConnections.Count - 1; j >= 0; j--)
                {
                    var pc = endPoints[i].PassiveConnections[j];
                    AddDeviceHierarchal(graph, db.FindById<PatchPanel>(pc.PatchPanel), out string ppKey);
                    graph.AddLink(new GraphLink(lastKey, ppKey, id));
                    lastKey = ppKey;
                }
                AddDeviceHierarchal(graph, db.FindById<Device>(endPoints[i].Device), out string deviceKey);
                graph.AddLink(new GraphLink(lastKey, deviceKey, id));
                lastKey = lastFirstEndPointKey;
            }

            return Json(graph, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}