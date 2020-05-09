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
using Omu.ValueInjecter;
using TciCommon.Models;
using TciDataLinks.Models;

namespace TciDataLinks.Controllers
{
    [Authorize]
    public class ConnectionController : BaseController
    {
        private readonly IEnumerable<City> cities = null;
        private readonly DeviceController deviceController;
        public ConnectionController(IDbContext db, IEnumerable<City> cities, DeviceController deviceController) : base(db) 
        {
            this.cities = cities;
            this.deviceController = deviceController;
        }

        public IActionResult Item(ObjectId id)
        {
            var conn = db.FindById<Connection>(id);
            var vm = Mapper.Map<ConnectionViewModel>(conn);
            foreach (var ep in db.Find<EndPoint>(e => e.Connection == id).SortBy(e => e.Index).ToEnumerable())
                vm.EndPoints.Add(Mapper.Map<EndPointViewModel>(ep));
            return View(vm);
        }

        public IActionResult Index()
        {
            return View(new ConnectionSearchViewModel());
        }

        private const int SEARCH_RESULT_LIMIT = 200;

        [HttpPost]
        public IActionResult Index(ConnectionSearchViewModel model)
        {
            List<ObjectId> devices;
            if (ObjectId.TryParse(model.Device, out ObjectId deviceId))
                devices = new List<ObjectId> { deviceId };
            else
            {
                ObjectId parentId;
                PlaceType parentType;
                if (ObjectId.TryParse(model.Rack, out parentId))
                    parentType = PlaceType.Rack;
                else if (ObjectId.TryParse(model.Room, out parentId))
                    parentType = PlaceType.Room;
                else if (ObjectId.TryParse(model.Building, out parentId))
                    parentType = PlaceType.Building;
                else if (ObjectId.TryParse(model.Center, out parentId))
                    parentType = PlaceType.Center;
                else
                    throw new NotImplementedException();
                devices = deviceController.FindDevices(parentId, parentType).Select(d => d.Id).ToList();
            }

            var fb = Builders<EndPoint>.Filter;
            var filters = new List<FilterDefinition<EndPoint>>
            {
                fb.In(e => e.Device, devices)
            };
            if (model.SearchType == ConnectionSearchViewModel.EndPointSearchType.First)
                filters.Add(fb.Eq(e => e.Index, 0));
            else if (model.SearchType == ConnectionSearchViewModel.EndPointSearchType.NotFirst)
                filters.Add(fb.Gt(e => e.Index, 0));

            var connections = db.Aggregate<EndPoint>()
                .Match(fb.And(filters))
                .Group(id => id.Connection, g => new { g.Key })
                .Lookup(nameof(Connection), "Key", "_id", "as")
                .Unwind("as").ReplaceRoot<Connection>("$as")
                .Limit(SEARCH_RESULT_LIMIT)
                .ToList();
            model.SearchResult = connections.Select(c => Mapper.Map<ConnectionViewModel>(c)).ToList();
            foreach (var c in model.SearchResult)
            {
                c.EndPoints = db.Find<EndPoint>(e => e.Connection == c.Id).SortBy(e => e.Index).ToEnumerable()
                    .Select(e => Mapper.Map<EndPointViewModel>(e)).ToList();
            }
            model.City = null;
            return View(model);
        }

        public IActionResult Add()
        {
            return View(new ConnectionViewModel());
        }

        [HttpPost]
        public IActionResult Add(ConnectionViewModel model)
        {
            var connection = Mapper.Map<Connection>(model);
            db.Save(connection);
            foreach (var evm in model.EndPoints)
            {
                var e = Mapper.Map<EndPoint>(evm);
                e.Connection = connection.Id;
                db.Save(e);
            }
            return RedirectToAction(nameof(Edit), new { id = connection.Id });
        }

        public IActionResult Edit(ObjectId id)
        {
            var connection = db.FindById<Connection>(id);
            var model = Mapper.Map<ConnectionViewModel>(connection);
            foreach (var e in db.Find<EndPoint>(e => e.Connection == id).SortBy(e => e.Index).ToEnumerable())
            {
                var evm = Mapper.Map<EndPointViewModel>(e);
                var rackId = db.FindById<Device>(evm.Device).Rack;
                var roomId = db.FindById<Rack>(rackId).Parent;
                var buildingId = db.FindById<Room>(roomId).Parent;
                evm.Building = buildingId;
                foreach (var pvm in evm.PassiveConnectionViewModels)
                    pvm.EndPointIndex = e.Index;
                model.EndPoints.Add(evm);
            }
            ViewData.Add("EditMode", true);
            return View("Add", model);
        }

        [HttpPost]
        public IActionResult Edit(ConnectionViewModel model)
        {
            var connection = Mapper.Map<Connection>(model);
            db.Save(connection);
            var endPointsId = new List<ObjectId>();
            int i = 0;
            foreach (var evm in model.EndPoints)
            {
                var e = Mapper.Map<EndPoint>(evm);
                e.Index = i++;
                e.Connection = connection.Id;
                db.Save(e);
                endPointsId.Add(e.Id);
            }
            var deletedEndPoints = db.Find<EndPoint>(e => e.Connection == connection.Id).Project(e => e.Id).ToEnumerable()
                .Where(e => !endPointsId.Contains(e));
            foreach (var e in deletedEndPoints)
                db.DeleteOne<EndPoint>(e);

            return RedirectToAction(nameof(Edit), new { id = connection.Id });
        }

        public IActionResult AddEndPoint(int index, ObjectId building, ObjectId device)
        {
            return GetEditorTemplatePartialView<EndPoint>(new EndPointViewModel
            { 
                Index = index, 
                Building = building,
                Device = device
            });
        }

        public IActionResult AddPassiveConnection(int endPointIndex, int index, ObjectId patchPanel)
        {
            return GetEditorTemplatePartialView<PassiveConnection>(new PassiveConnectionViewModel
            {
                EndPointIndex = endPointIndex,
                Index = index,
                PatchPanel = patchPanel
            });
        }

        //[AcceptVerbs("GET", "POST")]
        //public IActionResult PortNumberIsValid(string portNumber, ObjectId device, ObjectId patchPanel)
        //{
        //    if (device != ObjectId.Empty)
        //    {
        //        var exists = db.Any<Connection>(c => c.EndPoints.Any(e => e.Device == device && e.PortNumber == portNumber));
        //        return Json(!exists);
        //    }
        //    else if (patchPanel != ObjectId.Empty)
        //    {
        //        var exists = db.Any<Connection>(c => c.EndPoints.Any(e => e.PassiveConnections.Any(p => p.PatchPanel == patchPanel && p.PortNumber == portNumber)));
        //        return Json(!exists);
        //    }
        //    //TODO
        //    return Json(true);
        //}

        public IActionResult Graph(ObjectId center)
        {
            var graph = new Graph();
            var centerObj = db.FindById<CommCenter>(center);
            var centerNode = new GraphNode 
            { 
                key = "Center_" + centerObj.Id, 
                text = "مرکز " + centerObj.Name, 
                isGroup = true 
            };
            graph.Nodes.Add(centerNode);
            var deviceIds = new List<ObjectId>();
            foreach (var building in db.FindGetResults<Building>(b => b.Parent == center))
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
                var endPoints = c.ToList();
                string lastKey = null;
                for (int i = 0; i < endPoints.Count; i++)
                {
                    var ep = endPoints[i];
                    var device = db.FindById<Device>(ep.Device);
                    var deviceKey = "Device_" + ep.Device;
                    CheckDeviceParents(graph, device);
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

        private void CheckDeviceParents(Graph graph, Device device)
        {
            var deviceKey = "Device_" + device.Id;
            if (graph.Nodes.Any(n => n.key == deviceKey))
                return;
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
            if(!graph.Nodes.Any(n => n.key == buildingKey))
                graph.Nodes.Add(new GraphNode { key = buildingKey, text = "ساختمان " + building.Name, group = centerKey, isGroup = true });
            if (!graph.Nodes.Any(n => n.key == roomKey))
                graph.Nodes.Add(new GraphNode { key = roomKey, text = "سالن " + room.Name, group = buildingKey, isGroup = true });
            if (!graph.Nodes.Any(n => n.key == rackKey))
                graph.Nodes.Add(new GraphNode { key = rackKey, text = "راک " + rack.ToString(), group = roomKey, isGroup = true });
            graph.Nodes.Add(new GraphNode { key = deviceKey, text = device.ToString(), group = rackKey });
        }

        public IActionResult ConnectionGraph(ObjectId id)
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