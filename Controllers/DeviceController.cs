﻿using EasyMongoNet;
using EasyMongoNet.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class DeviceController : BaseController
    {
        private readonly IEnumerable<City> cities = null;
        public DeviceController(IDbContext db, IEnumerable<City> cities) : base(db)
        {
            this.cities = cities;
        }

        public IActionResult Item(string id)
        {
            var device = db.FindById<Device>(id);
            var model = Mapper.Map<DeviceViewModel>(device);
            model.UsedPorts = db.Find<EndPoint>(e => e.Device == id)
                .SortBy(e => e.PortNumber)
                .Project(e => new PortViewModel { Connection = e.Connection, PortNumber = e.PortNumber, EndPointIndex = e.Index })
                .ToList();
            if (UserPermissions.Contains(Permission.ViewUserLogs))
            {
                var users = db.All<AuthUserX>().ToDictionary(u => u.Username, u => u.DisplayName);
                model.Logs = db.Find<UserActivity>(a => a.ObjId == id).SortBy(a => a.Time)
                    .Project(a => new UserActivityViewModel
                    {
                        User = users.ContainsKey(a.Username) ? users[a.Username] : a.Username,
                        Time = a.Time,
                        ActivityType = a.ActivityType,
                        ObjId = a.ObjId
                    })
                    .ToList();
            }
            return View(model);
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        public IActionResult Add(string city, string center, string building, string room, string rack)
        {
            var model = new DeviceViewModel
            {
                City = city,
                Center = center,
                Building = building.ToString(),
                Room = room.ToString(),
                Place = rack
            };
            ViewBag.Cities = cities;
            if (city != null)
            {
                ViewBag.Centers = db.Find<CommCenter>(c => c.City == city).Project(c => new { c.Id, c.Name }).ToEnumerable()
                    .Select(c => new SelectListItem(text: c.Name, value: c.Id.ToString(), selected: c.Id == center));
            }
            if (center != null)
            {
                ViewBag.Buildings = db.FindGetResults<Building>(b => b.Parent == center)
                    .Select(b => new SelectListItem(text: b.Name, value: b.Id.ToString(), selected: b.Id == building));
            }
            if (building != null)
            {
                ViewBag.Rooms = db.FindGetResults<Room>(r => r.Parent == building)
                    .Select(r => new SelectListItem(text: r.Name, value: r.Id.ToString(), selected: r.Id == room));
            }
            if (rack != null)
            {
                var rackObj = db.FindById<Rack>(rack);
                model.Place = rack;
                model.RackLine = rackObj.Line;
                model.RackIndex = rackObj.Index;
                model.RackSide = rackObj.Side;
            }
            return View(model);
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        [HttpPost]
        public IActionResult Add(DeviceViewModel m)
        {
            ObjectId buildingId, roomId, rackId;
            if (m.Center == null)
            {
                ViewBag.Cities = cities;
                ModelState.AddModelError("Center", "مرکز درست انتخاب نشده است.");
                return View(m);
            }

            if (!ObjectId.TryParse(m.Building, out buildingId))
            {
                var building = new Building { Name = m.Building, Parent = m.Center };
                db.Save(building);
                buildingId = ObjectId.Parse(building.Id);
            }
            if (!ObjectId.TryParse(m.Room, out roomId))
            {
                var room = new Room { Name = m.Room, Parent = buildingId.ToString() };
                db.Save(room);
                roomId = ObjectId.Parse(room.Id);
            }
            rackId = ObjectId.Parse(db.Find<Rack>(r => r.Parent == roomId.ToString() && r.Line == m.RackLine && r.Index == m.RackIndex && r.Side == m.RackSide)
                .Project(r => r.Id).FirstOrDefault());
            if (rackId == ObjectId.Empty)
            {
                var rack = new Rack { Parent = roomId.ToString(), Line = m.RackLine, Index = m.RackIndex, Side = m.RackSide };
                db.Save(rack);
                rackId = ObjectId.Parse(rack.Id);
            }

            var device = Mapper.Map<Device>(m);
            device.Place = rackId.ToString();

            db.Save(device);
            return RedirectToAction("Item", "Place", new { type = "Rack", id = rackId.ToString() });
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        public IActionResult Edit(string id)
        {
            var device = db.FindById<Device>(id);
            var model = Mapper.Map<DeviceViewModel>(device);
            var rack = db.FindById<Rack>(device.Place);
            model.RackLine = rack.Line;
            model.RackIndex = rack.Index;
            model.RackSide = rack.Side;
            var parent = rack.Parent;
            model.Room = parent.ToString();
            parent = db.FindById<Room>(parent).Parent;
            model.Building = parent.ToString();
            parent = db.FindById<Building>(parent).Parent;
            model.Center = parent;
            model.City = db.Find<CommCenter>(c => c.Id == parent).Project(c => c.City).FirstOrDefault();

            ViewBag.Cities = cities;
            ViewBag.Centers = db.FindGetResults<CommCenter>(c => c.City == model.City)
                .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == model.Center });
            ViewBag.Buildings = db.FindGetResults<Building>(b => b.Parent == model.Center)
                .Select(b => new SelectListItem { Text = b.Name, Value = b.Id.ToString(), Selected = b.Id.ToString() == model.Building });
            ViewBag.Rooms = db.FindGetResults<Room>(r => r.Parent == model.Building)
                .Select(r => new SelectListItem { Text = r.Name, Value = r.Id.ToString(), Selected = r.Id.ToString() == model.Room });
            return View(model);
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        [HttpPost]
        public IActionResult Edit(DeviceViewModel m)
        {
            ObjectId buildingId, roomId, rackId;
            if (m.Center == null)
            {
                ViewBag.Cities = cities;
                ModelState.AddModelError("Center", "مرکز درست انتخاب نشده است.");
                return View(m);
            }
            if (!ObjectId.TryParse(m.Building, out buildingId))
            {
                var building = new Building { Name = m.Building, Parent = m.Center };
                db.Save(building);
                buildingId = ObjectId.Parse(building.Id);
            }
            if (!ObjectId.TryParse(m.Room, out roomId))
            {
                var room = new Room { Name = m.Room, Parent = buildingId.ToString() };
                db.Save(room);
                roomId = ObjectId.Parse(room.Id);
            }
            rackId = ObjectId.Parse(db.Find<Rack>(r => r.Parent == roomId.ToString() && r.Line == m.RackLine && r.Index == m.RackIndex && r.Side == m.RackSide)
                .Project(r => r.Id).FirstOrDefault());
            if (rackId == ObjectId.Empty)
            {
                var rack = new Rack { Parent = roomId.ToString(), Line = m.RackLine, Index = m.RackIndex, Side = m.RackSide };
                db.Save(rack);
                rackId = ObjectId.Parse(rack.Id);
            }
            var device = db.FindById<Device>(m.Id);
            device.InjectFrom(m);
            device.Place = rackId.ToString();
            db.Save(device);
            return RedirectToAction("Item", "Place", new { type = "Rack", id = rackId.ToString() });
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult DeviceAddressIsValid(string id, string address)
        {
            bool exists = db.Any<Device>(d => d.Id != id && d.Address == address);
            return Json(!exists);
        }

        public IEnumerable<Device> FindDevices(string parentId, PlaceType parentType)
        {
            if (parentType == PlaceType.Rack)
                return db.FindGetResults<Device>(d => d.Place == parentId);
            else
            {
                List<string> racks;
                if (parentType == PlaceType.Room)
                    racks = db.Find<Rack>(r => r.Parent == parentId).Project(r => r.Id).ToList();
                else if (parentType == PlaceType.Building)
                {
                    var rooms = db.Find<Room>(r => r.Parent == parentId).Project(r => r.Id).ToList();
                    racks = db.Find<Rack>(r => rooms.Contains(r.Parent)).Project(r => r.Id).ToList();
                }
                else if (parentType == PlaceType.Center)
                {
                    var buildings = db.Find<Building>(b => b.Parent == parentId).Project(b => b.Id).ToList();
                    var rooms = db.Find<Room>(r => buildings.Contains(r.Parent)).Project(r => r.Id).ToList();
                    racks = db.Find<Rack>(r => rooms.Contains(r.Parent)).Project(r => r.Id).ToList();
                }
                else if (parentType == PlaceType.City)
                {
                    var centers = db.Find<CommCenter>(c => c.City == parentId).Project(c => c.Id).ToList();
                    var buildings = db.Find<Building>(b => centers.Contains(b.Parent)).Project(b => b.Id).ToList();
                    var rooms = db.Find<Room>(r => buildings.Contains(r.Parent)).Project(r => r.Id).ToList();
                    racks = db.Find<Rack>(r => rooms.Contains(r.Parent)).Project(r => r.Id).ToList();
                }
                else
                    throw new NotImplementedException();
                return db.FindGetResults<Device>(d => racks.Contains(d.Place));
            }
        }

        public IActionResult Delete(string id)
        {
            var device = db.FindById<Device>(id);
            if (!db.Any<EndPoint>(e => e.Device == id))
                db.DeleteOne<Device>(id);
            return RedirectToAction("Item", "Place", new { type = "Rack", id = device.Place.ToString() });
        }
    }
}