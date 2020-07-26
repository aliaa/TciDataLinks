using EasyMongoNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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


        private static PlaceType[] ForbiddenPlaces = new PlaceType[] { PlaceType.City, PlaceType.Device, PlaceType.Passive };
        public IActionResult Place(PlaceType type, ObjectId id, Device.NetworkType? networkType = null, bool? hasCustomer = null)
        {
            if (ForbiddenPlaces.Contains(type))
                throw new Exception("type: " + type + " is not supported for creating graph!");

            var graph = new Graph();
            CommCenter center;
            var deviceIds = new List<ObjectId>();

            IEnumerable<PlaceBase> buildings = null, rooms = null, racks = null;
            if (type == PlaceType.Center)
            {
                center = db.FindById<CommCenter>(id);
                buildings = db.FindGetResults<Building>(b => b.Parent == id);
            }
            else if (type == PlaceType.Building)
            {
                var building = db.FindById<Building>(id);
                center = db.FindById<CommCenter>(building.Parent);
                buildings = new Building[] { building };
            }
            else if (type == PlaceType.Room)
            {
                var room = db.FindById<Room>(id);
                var building = db.FindById<Building>(room.Parent);
                center = db.FindById<CommCenter>(building.Parent);
                buildings = new Building[] { building };
                rooms = new Room[] { room };
            }
            else if (type == PlaceType.Rack)
            {
                var rack = db.FindById<Rack>(id);
                var room = db.FindById<Room>(rack.Parent);
                var building = db.FindById<Building>(room.Parent);
                center = db.FindById<CommCenter>(building.Parent);
                buildings = new Building[] { building };
                rooms = new Room[] { room };
                racks = new Rack[] { rack };
            }
            else
                throw new NotImplementedException();

            var centerNode = new GraphNode
            {
                key = "Center_" + center.Id,
                text = "مرکز " + center.Name,
                isGroup = true
            };
            graph.AddNode(centerNode);

            foreach (var building in buildings)
            {
                var buildingNode = new GraphNode
                {
                    key = "Building_" + building.Id,
                    text = "ساختمان " + building.Name,
                    group = centerNode.key,
                    isGroup = true
                };
                graph.AddNode(buildingNode);

                if (rooms == null)
                    rooms = db.FindGetResults<Room>(r => r.Parent == building.Id);
                foreach (var room in rooms)
                {
                    var roomNode = new GraphNode
                    {
                        key = "Room_" + room.Id,
                        text = "سالن " + room.Name,
                        group = buildingNode.key,
                        isGroup = true
                    };
                    graph.AddNode(roomNode);

                    if (racks == null)
                        racks = db.FindGetResults<Rack>(r => r.Parent == room.Id);
                    foreach (var rack in racks)
                    {
                        var rackNode = new GraphNode
                        {
                            key = "Rack_" + rack.Id,
                            text = rack.ToString(),
                            group = roomNode.key,
                            isGroup = true
                        };
                        graph.AddNode(rackNode);

                        var filter = Builders<Device>.Filter.Eq(d => d.Rack, rack.Id);
                        if (networkType != null)
                            filter = Builders<Device>.Filter.And(filter, Builders<Device>.Filter.Eq(d => d.Network, networkType));
                        foreach (var device in db.Find(filter).ToEnumerable())
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
                        foreach (var pp in db.FindGetResults<Passive>(p => p.Rack == rack.Id))
                        {
                            var ppNode = new GraphNode
                            {
                                key = "Passive_" + pp.Id,
                                text = pp.ToString(),
                                group = rackNode.key
                            };
                            graph.AddNode(ppNode);
                        }
                    }
                    racks = null;
                }
                rooms = null;
            }
 
            var connections = db.FindGetResults<EndPoint>(e => deviceIds.Contains(e.Device))
                .GroupBy(key => key.Connection)
                .Select(c => new
                {
                    Connection = db.FindById<Connection>(c.Key),
                    EndPoints = db.Find<EndPoint>(e => e.Connection == c.Key).SortBy(e => e.Index).ToList()
                });
            if (hasCustomer == true)
                connections = connections.Where(c => c.Connection.CustomerId != ObjectId.Empty);
            else if(hasCustomer == false)
                connections = connections.Where(c => c.Connection.CustomerId == ObjectId.Empty);

            foreach (var c in connections)
            {
                string lastKey = null;
                string lastPort = c.EndPoints[0].PortNumber;

                var device = db.FindById<Device>(c.EndPoints[0].Device);
                AddDeviceHierarchal(graph, device, out string deviceKey, out string anotherCenter, centerNode.key);
                lastKey = deviceKey;
                if (anotherCenter == null)
                {
                    foreach (var pc in c.EndPoints[0].PassiveConnections)
                    {
                        AddDeviceHierarchal(graph, db.FindById<Passive>(pc.PatchPanel), out string ppKey);
                        graph.AddLink(new GraphLink(lastKey, ppKey, c.Connection, lastPort, pc.PortNumber));
                        lastKey = ppKey;
                        lastPort = pc.PortNumber;
                    }
                }
                else
                    lastKey = anotherCenter;
                var lastFirstEndPointKey = lastKey;
                var lastFirstEndPointPort = lastPort;

                for (int i = 1; i < c.EndPoints.Count; i++)
                {
                    var ep = c.EndPoints[i];

                    device = db.FindById<Device>(ep.Device);
                    AddDeviceHierarchal(graph, device, out deviceKey, out anotherCenter, centerNode.key);
                    if (anotherCenter != null)
                    {
                        if (lastKey != anotherCenter)
                        {
                            graph.AddLink(new GraphLink(lastFirstEndPointKey, anotherCenter, c.Connection, lastPort, ep.PortNumber));
                            lastKey = lastFirstEndPointKey;
                            lastPort = lastFirstEndPointPort;
                        }
                        continue;
                    }

                    for (int j = ep.PassiveConnections.Count - 1; j >= 0; j--)
                    {
                        var pc = ep.PassiveConnections[j];
                        AddDeviceHierarchal(graph, db.FindById<Passive>(pc.PatchPanel), out string ppKey);
                        graph.AddLink(new GraphLink(lastKey, ppKey, c.Connection, lastPort, pc.PortNumber));
                        lastKey = ppKey;
                        lastPort = pc.PortNumber;
                    }
                    graph.AddLink(new GraphLink(lastKey, deviceKey, c.Connection, lastPort, c.EndPoints[i].PortNumber));
                    lastKey = lastFirstEndPointKey;
                    lastPort = lastFirstEndPointPort;
                }

                if (c.Connection.CustomerId != ObjectId.Empty)
                {
                    var key = "Customer_" + c.Connection.CustomerId;
                    graph.AddNode(new GraphNode
                    {
                        key = key,
                        image = "/lib/bootstrap-icons/icons/person-fill.svg",
                        text = db.FindById<Customer>(c.Connection.CustomerId)?.ToString()
                    });
                    graph.AddLink(new GraphLink(lastKey, key, c.Connection.Id, "ارتباط مشتری"));
                }
            }

            // Add empty nodes to empty groups:
            int n = 0;
            foreach (var item in graph.Nodes.Where(n => n.isGroup && !graph.Nodes.Any(nn => nn.group == n.key)).ToList())
            {
                graph.AddNode(new GraphNode
                {
                    key = "Empty_" + item.key,
                    text = "",
                    group = item.key
                });
                n++;
            }


            var locations = db.FindGetResults<NodeLocationWithKey>(x => x.Center == id).ToDictionary(k => k.Key, v => v.Loc);
            graph.SetLocations(locations);
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
            else if (device is Passive)
                deviceKey = "Passive_" + device.Id;
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
            graph.AddNode(new GraphNode { key = rackKey, text = rack.ToString(), group = roomKey, isGroup = true });
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
            var connection = db.FindById<Connection>(id);
            var endPoints = db.Find<EndPoint>(e => e.Connection == id).SortBy(e => e.Index).ToList();
            string lastKey = null;
            string lastPort = endPoints[0].PortNumber;

            AddDeviceHierarchal(graph, db.FindById<Device>(endPoints[0].Device), out lastKey);
            foreach (var pc in endPoints[0].PassiveConnections)
            {
                AddDeviceHierarchal(graph, db.FindById<Passive>(pc.PatchPanel), out string ppKey);
                graph.AddLink(new GraphLink(lastKey, ppKey, connection, lastPort, pc.PortNumber));
                lastKey = ppKey;
                lastPort = pc.PortNumber;
            }
            var lastFirstEndPointKey = lastKey;

            for (int i = 1; i < endPoints.Count; i++)
            {
                for (int j = endPoints[i].PassiveConnections.Count - 1; j >= 0; j--)
                {
                    var pc = endPoints[i].PassiveConnections[j];
                    AddDeviceHierarchal(graph, db.FindById<Passive>(pc.PatchPanel), out string ppKey);
                    graph.AddLink(new GraphLink(lastKey, ppKey, connection, lastPort, pc.PortNumber));
                    lastKey = ppKey;
                    lastPort = pc.PortNumber;
                }
                AddDeviceHierarchal(graph, db.FindById<Device>(endPoints[i].Device), out string deviceKey);
                graph.AddLink(new GraphLink(lastKey, deviceKey, connection, lastPort, endPoints[i].PortNumber));
                lastKey = lastFirstEndPointKey;
            }

            if(connection.CustomerId != ObjectId.Empty)
            {
                var key = "Customer_" + connection.CustomerId;
                graph.AddNode(new GraphNode
                {
                    key = key,
                    image = "/lib/bootstrap-icons/icons/person-fill.svg",
                    text = db.FindById<Customer>(connection.CustomerId)?.ToString()
                });
                graph.AddLink(new GraphLink(lastKey, key, connection.Id, "ارتباط مشتری"));
            }

            return Json(graph, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public class Req
        {
            public ObjectId center { get; set; }
            public List<NodeLocationWithKey> nodeLocations { get; set; }
        }

        [Authorize(nameof(Permission.ChangeGraphOrders))]
        [HttpPost]
        public IActionResult SaveOrders([FromBody] Req req)
        {
            lock (db)
            {
                var existing = db.Find<NodeLocationWithKey>(x => x.Center == req.center).ToEnumerable().ToDictionary(k => k.Key);
                foreach (var item in req.nodeLocations)
                {
                    item.Center = req.center;
                    if (existing.ContainsKey(item.Key))
                        existing[item.Key].Loc = item.Loc;
                    else
                        existing.Add(item.Key, item);
                }
                foreach (var item in existing.Values)
                {
                    db.Save(item);
                }
                return Ok();
            }
        }
    }
}