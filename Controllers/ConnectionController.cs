﻿using AliaaCommon;
using EasyMongoNet;
using EasyMongoNet.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Linq;
using TciCommon.Models;
using TciDataLinks.Models;
using TciDataLinks.ViewModels;

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
            var vm = ConnectionToViewModel(conn);
            return View(vm);
        }

        private ConnectionViewModel ConnectionToViewModel(Connection c, bool addMoreDetails = false)
        {
            var vm = Mapper.Map<ConnectionViewModel>(c);
            if (c.CustomerId != ObjectId.Empty)
                vm.Customer = db.FindById<Customer>(c.CustomerId);
            foreach (var e in db.Find<EndPoint>(e => e.Connection == c.Id).SortBy(e => e.Index).ToEnumerable())
            {
                var evm = Mapper.Map<EndPointViewModel>(e);
                if (addMoreDetails)
                {
                    var rackId = db.FindById<Device>(evm.Device).Rack;
                    var roomId = db.FindById<Rack>(rackId).Parent;
                    var buildingId = db.FindById<Room>(roomId).Parent;
                    var centerId = db.FindById<Building>(buildingId).Parent;
                    evm.Center = centerId;
                }
                vm.EndPoints.Add(evm);
            }

            var endPointsIds = vm.EndPoints.Select(e => e.Id).ToList();
            var createActivity = db.FindFirst<UserActivity>(a => endPointsIds.Contains(a.ObjId) && a.ActivityType == ActivityType.Insert);
            var lastEditActivity = db.Find<UserActivity>(a => endPointsIds.Contains(a.ObjId) && a.ActivityType == ActivityType.Update)
                .Project(a => new { a.Time, a.Username })
                .SortByDescending(a => a.Time).FirstOrDefault();
            if (createActivity != null)
            {
                vm.CreateDate = PersianDateUtils.GetPersianDateString(createActivity.Time);
                var user = db.FindFirst<AuthUserX>(u => u.Username == createActivity.Username);
                if (user != null)
                    vm.CreatedUser = user.DisplayName;
            }
            if (lastEditActivity != null)
            {
                vm.LastEditDate = PersianDateUtils.GetPersianDateString(lastEditActivity.Time);
                var user = db.FindFirst<AuthUserX>(u => u.Username == lastEditActivity.Username);
                if (user != null)
                    vm.EditedUser = user.DisplayName;
            }
            return vm;
        }

        public IActionResult Index()
        {
            return View(new ConnectionSearchViewModel
            {
                TotalLinksCount = db.Count<Connection>()
            });
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
                else if (ObjectId.TryParse(model.City, out parentId))
                    parentType = PlaceType.City;
                else
                    throw new NotImplementedException();
                devices = deviceController.FindDevices(parentId, parentType).Select(d => d.Id).ToList();
            }

            var fb = Builders<EndPoint>.Filter;
            var filters = new List<FilterDefinition<EndPoint>>
            {
                fb.In(e => e.Device, devices)
            };

            if (model.PortType != null)
                filters.Add(fb.Eq(e => e.PortType, model.PortType.Value));
            if (model.Module != null)
                filters.Add(fb.Eq(e => e.Module, model.Module.Value));
            if (model.PatchCord != null)
                filters.Add(fb.Eq(e => e.PatchCord, model.PatchCord.Value));
            if (model.Connector != null)
                filters.Add(fb.Eq(e => e.Connector, model.Connector.Value));
            if (model.DataProtection != null)
                filters.Add(fb.Eq(e => e.DataProtection, model.DataProtection.Value));
            if (model.TransmissionProtection != null)
                filters.Add(fb.Eq(e => e.TransmissionProtection, model.TransmissionProtection.Value));
            if (model.Incomplete != null)
                filters.Add(fb.Eq(e => e.Incomplete, model.Incomplete.Value));

            if (model.SearchType == ConnectionSearchViewModel.EndPointSearchType.First)
                filters.Add(fb.Eq(e => e.Index, 0));
            else if (model.SearchType == ConnectionSearchViewModel.EndPointSearchType.NotFirst)
                filters.Add(fb.Gt(e => e.Index, 0));

            var agg = db.Aggregate<EndPoint>()
                .Match(fb.And(filters));
            if (model.NetworkType != ConnectionSearchViewModel.DeviceNetworkType.All &&
                model.NetworkType != ConnectionSearchViewModel.DeviceNetworkType.InterNetwork)
            {
                agg = agg.Lookup(nameof(Device), nameof(EndPoint.Device), "_id", "device")
                    .Unwind("device")
                    .Match("{\"device." + nameof(Device.Network) + "\": \"" + model.NetworkType.ToString() + "\" }")
                    .Project("{\"device\": 0 }")
                    .As<EndPoint>();
            }
            var connections = agg.Group(id => id.Connection, g => new { g.Key })
                .Lookup(nameof(Connection), "Key", "_id", "as")
                .Unwind("as").ReplaceRoot<Connection>("$as")
                .SortByDescending(c => c.IdInt)
                .Limit(SEARCH_RESULT_LIMIT)
                .ToList();
            var cvmList = connections.Select(c => ConnectionToViewModel(c));
            if (model.NetworkType == ConnectionSearchViewModel.DeviceNetworkType.InterNetwork)
                cvmList = cvmList.Where(x => x.EndPoints.Select(e => db.FindById<Device>(e.Device)).GroupBy(d => d.Network).Count() > 1);

            model.SearchResult = cvmList.ToList();
            model.TotalLinksCount = db.Count<Connection>();
            model.City = null;
            return View(model);
        }

        [Authorize(nameof(Permission.EditConnections))]
        public IActionResult Add(ObjectId device)
        {
            var vm = new ConnectionViewModel();
            if (device != ObjectId.Empty)
            {
                var deviceObj = db.FindById<Device>(device);
                var rack = db.FindById<Rack>(deviceObj.Rack);
                var room = db.FindById<Room>(rack.Parent);
                var building = db.FindById<Building>(room.Parent);
                vm.EndPoints.Add(new EndPointViewModel { Center = building.Parent, Device = device });
            }
            return View(vm);
        }

        [Authorize(nameof(Permission.EditConnections))]
        [HttpPost]
        public IActionResult Add(ConnectionViewModel model)
        {
            var connection = Mapper.Map<Connection>(model);
            connection.IdInt = db.Find<Connection>(_ => true).SortByDescending(c => c.IdInt).Project(c => c.IdInt).FirstOrDefault() + 1;
            db.Save(connection);
            foreach (var evm in model.EndPoints)
            {
                var e = Mapper.Map<EndPoint>(evm);
                e.Connection = connection.Id;
                db.Save(e);
            }
            return RedirectToAction(nameof(Edit), new { id = connection.Id });
        }

        [Authorize(nameof(Permission.EditConnections))]
        public IActionResult Edit(ObjectId id)
        {
            var c = db.FindById<Connection>(id);
            var vm = ConnectionToViewModel(c, addMoreDetails: true);
            ViewData.Add("EditMode", true);
            return View("Add", vm);
        }

        [Authorize(nameof(Permission.EditConnections))]
        [HttpPost]
        public IActionResult Edit(ConnectionViewModel model)
        {
            db.UpdateOne<Connection>(c => c.Id == model.Id, Builders<Connection>.Update.Set(c => c.CustomerId, model.CustomerId));
            var endPointsId = new List<ObjectId>();
            int i = 0;
            foreach (var evm in model.EndPoints)
            {
                var e = Mapper.Map<EndPoint>(evm);
                if (e.Device == ObjectId.Empty)
                    continue;
                e.PassiveConnections = e.PassiveConnections.Where(pc => pc.PatchPanel != ObjectId.Empty).ToList();
                e.Index = i++;
                e.Connection = model.Id;
                db.Save(e);
                endPointsId.Add(e.Id);
            }
            var deletedEndPoints = db.Find<EndPoint>(e => e.Connection == model.Id).Project(e => e.Id).ToEnumerable()
                .Where(e => !endPointsId.Contains(e));
            foreach (var e in deletedEndPoints)
                db.DeleteOne<EndPoint>(e);

            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        [Authorize(nameof(Permission.EditConnections))]
        public IActionResult AddEndPoint(int index, ObjectId center, ObjectId device)
        {
            return GetEditorTemplatePartialView<EndPoint>(new EndPointViewModel
            {
                Index = index,
                Center = center,
                Device = device
            });
        }

        [Authorize(nameof(Permission.EditConnections))]
        public IActionResult AddPassiveConnection(int endPointIndex, int index, ObjectId passive)
        {
            return GetEditorTemplatePartialView<PassiveConnection>(new PassiveConnectionViewModel
            {
                EndPointIndex = endPointIndex,
                Index = index,
                PatchPanel = passive
            });
        }

        [Authorize(nameof(Permission.EditConnections))]
        public IActionResult Delete(ObjectId id)
        {
            db.DeleteMany<EndPoint>(e => e.Connection == id);
            db.DeleteOne<Connection>(c => c.Id == id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CustomerSearch(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Ok();
            var fb = Builders<Customer>.Filter;
            FilterDefinition<Customer> filter;
            if (long.TryParse(term, out long docNum))
                filter = fb.Eq(c => c.DocumentNumber, docNum);
            else
                filter = fb.Regex(c => c.CustomerName, new BsonRegularExpression(term.Trim().Replace(" ", ".*")));
            var results = db.Find<Customer>(fb.And(filter, fb.Ne(c => c.IsAborted, true))).ToEnumerable()
                .Select(c => new { id = c.Id.ToString(), text = c.ToString() });

            return Json(new { results });
        }
    }
}