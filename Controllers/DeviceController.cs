using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyMongoNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;
using MongoDB.Driver;
using Omu.ValueInjecter;
using TciCommon.Models;
using TciDataLinks.Models;
using TciDataLinks.ViewModels;

namespace TciDataLinks.Controllers
{
    [Authorize]
    public class DeviceController : BaseController
    {
        private IEnumerable<City> cities = null;
        public DeviceController(IDbContext db, IEnumerable<City> cities) : base(db) 
        {
            this.cities = cities;
        }

        public IActionResult Item(ObjectId id)
        {
            var device = db.FindById<Device>(id);
            var model = Mapper.Map<DeviceViewModel>(device);
            return View(model);
        }

        //public IActionResult Add()
        //{
        //    ViewBag.Cities = cities;
        //    return View();
        //}

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        public IActionResult Add(ObjectId city, ObjectId center, ObjectId building, ObjectId room, ObjectId rack)
        {
            var model = new DeviceViewModel
            {
                City = city,
                Center = center,
                Building = building.ToString(),
                Room = room.ToString(),
                Rack = rack
            };
            ViewBag.Cities = cities;
            if (city != ObjectId.Empty)
            {
                ViewBag.Centers = db.Find<CommCenter>(c => c.City == city).Project(c => new { c.Id, c.Name }).ToEnumerable()
                    .Select(c => new SelectListItem(text: c.Name, value: c.Id.ToString(), selected: c.Id == center));
            }
            if (center != ObjectId.Empty)
            {
                ViewBag.Buildings = db.FindGetResults<Building>(b => b.Parent == center)
                    .Select(b => new SelectListItem(text: b.Name, value: b.Id.ToString(), selected: b.Id == building));
            }
            if(building != ObjectId.Empty)
            {
                ViewBag.Rooms = db.FindGetResults<Room>(r => r.Parent == building)
                    .Select(r => new SelectListItem(text: r.Name, value: r.Id.ToString(), selected: r.Id == room));
            }
            if (rack != ObjectId.Empty)
            {
                var rackObj = db.FindById<Rack>(rack);
                model.Rack = rack;
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
            if (m.Center == ObjectId.Empty)
            {
                ViewBag.Cities = cities;
                ModelState.AddModelError("Center", "مرکز درست انتخاب نشده است.");
                return View(m);
            }

            if (!ObjectId.TryParse(m.Building, out buildingId))
            {
                var building = new Building { Name = m.Building, Parent = m.Center };
                db.Save(building);
                buildingId = building.Id;
            }
            if (!ObjectId.TryParse(m.Room, out roomId))
            {
                var room = new Room { Name = m.Room, Parent = buildingId };
                db.Save(room);
                roomId = room.Id;
            }
            rackId = db.Find<Rack>(r => r.Parent == roomId && r.Line == m.RackLine && r.Index == m.RackIndex && r.Side == m.RackSide)
                .Project(r => r.Id).FirstOrDefault();
            if (rackId == ObjectId.Empty)
            {
                var rack = new Rack { Parent = roomId, Line = m.RackLine, Index = m.RackIndex, Side = m.RackSide };
                db.Save(rack);
                rackId = rack.Id;
            }

            var device = Mapper.Map<Device>(m);
            device.Rack = rackId;

            db.Save(device);
            return RedirectToAction("Item", "Place", new { type = "Rack", id = rackId.ToString() });
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        public IActionResult Edit(string id)
        {
            var device = db.FindById<Device>(id);
            var model = Mapper.Map<DeviceViewModel>(device);
            var rack = db.FindById<Rack>(device.Rack);
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
            ViewBag.Rooms = db.FindGetResults<Room>(r => r.Parent == ObjectId.Parse(model.Building))
                .Select(r => new SelectListItem { Text = r.Name, Value = r.Id.ToString(), Selected = r.Id.ToString() == model.Room });
            return View(model);
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        [HttpPost]
        public IActionResult Edit(DeviceViewModel m)
        {
            ObjectId buildingId, roomId, rackId;
            if (m.Center == ObjectId.Empty)
            {
                ViewBag.Cities = cities;
                ModelState.AddModelError("Center", "مرکز درست انتخاب نشده است.");
                return View(m);
            }
            if (!ObjectId.TryParse(m.Building, out buildingId))
            {
                var building = new Building { Name = m.Building, Parent = m.Center };
                db.Save(building);
                buildingId = building.Id;
            }
            if (!ObjectId.TryParse(m.Room, out roomId))
            {
                var room = new Room { Name = m.Room, Parent = buildingId };
                db.Save(room);
                roomId = room.Id;
            }
            rackId = db.Find<Rack>(r => r.Parent == roomId && r.Line == m.RackLine && r.Index == m.RackIndex && r.Side == m.RackSide)
                .Project(r => r.Id).FirstOrDefault();
            if (rackId == ObjectId.Empty)
            {
                var rack = new Rack { Parent = roomId, Line = m.RackLine, Index = m.RackIndex, Side = m.RackSide };
                db.Save(rack);
                rackId = rack.Id;
            }
            var device = db.FindById<Device>(m.Id);
            device.InjectFrom(m);
            device.Rack = rackId;
            db.Save(device);
            return RedirectToAction("Item", "Place", new { type = "Rack", id = rackId.ToString() });
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult DeviceAddressIsValid(string id, string address)
        {
            ObjectId.TryParse(id, out ObjectId objId);
            bool exists = db.Any<Device>(d => d.Id != objId && d.Address == address);
            return Json(!exists);
        }

        public IEnumerable<Device> FindDevices(ObjectId parentId, PlaceType parentType)
        {
            if (parentType == PlaceType.Rack)
                return db.FindGetResults<Device>(d => d.Rack == parentId);
            else
            {
                List<ObjectId> racks;
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
                else
                    throw new NotImplementedException();
                return db.FindGetResults<Device>(d => racks.Contains(d.Rack));
            }
        }
    }
}