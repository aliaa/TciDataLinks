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

namespace TciDataLinks.Controllers
{
    [Authorize]
    public class DeviceController : BaseController
    {

        public DeviceController(IDbContext db, Settings settings) : base(db, settings) { }
        
        public IActionResult Add()
        {
            ViewBag.Cities = Cities;
            return View();
        }

        public IActionResult Centers(string city)
        {
            var centers = db.Find<CommCenter>(c => c.City == ObjectId.Parse(city))
                .Project(c => new { c.Id, c.Name }).ToEnumerable()
                .Select(c => new { value = c.Id.ToString(), text = c.Name });
            return Json(centers);
        }

        public IActionResult Buildings(string center)
        {
            var buildings = db.FindGetResults<Building>(b => b.Parent == ObjectId.Parse(center))
                .Select(r => new { id = r.Id.ToString(), text = r.Name });
            return Json(buildings);
        }

        public IActionResult Rooms(string building)
        {
            if (ObjectId.TryParse(building, out ObjectId buildingId))
            {
                var rooms = db.FindGetResults<Room>(r => r.Parent == buildingId)
                    .Select(r => new { id = r.Id.ToString(), text = r.Name });
                return Json(rooms);
            }
            return Json(Enumerable.Empty<object>());
        }

        public IActionResult Racks(string room)
        {
            if(ObjectId.TryParse(room, out ObjectId roomId))
            {
                var racks = db.FindGetResults<Rack>(r => r.Parent == roomId)
                    .Select(r => new { id = r.Id.ToString(), text = r.Name });
                return Json(racks);
            }
            return Json(Enumerable.Empty<object>());
        }

        [HttpPost]
        public IActionResult Add(DeviceViewModel m)
        {
            ObjectId centerId, buildingId, roomId, rackId;
            if (!ObjectId.TryParse(m.Center, out centerId))
            {
                ViewBag.Cities = Cities;
                ModelState.AddModelError("Center", "مرکز درست انتخاب نشده است.");
                return View(m);
            }
            if (!ObjectId.TryParse(m.Rack, out rackId))
            {
                if (!ObjectId.TryParse(m.Building, out buildingId))
                {
                    var building = new Building { Name = m.Building, Parent = centerId };
                    db.Save(building);
                    buildingId = building.Id;
                }
                if (!ObjectId.TryParse(m.Room, out roomId))
                {
                    var room = new Room { Name = m.Room, Parent = buildingId };
                    db.Save(room);
                    roomId = room.Id;
                }
                if (!ObjectId.TryParse(m.Rack, out rackId))
                {
                    var rack = new Rack { Name = m.Rack, Parent = roomId };
                    db.Save(rack);
                    rackId = rack.Id;
                }
            }

            var device = Mapper.Map<Device>(m);
            device.Rack = rackId;

            db.Save(device);
            return RedirectToAction("Item", "Place", new { type = "Rack", id = rackId.ToString() });
        }

        public IActionResult Edit(string id)
        {
            var device = db.FindById<Device>(id);
            var model = Mapper.Map<DeviceViewModel>(device);
            var parent = db.FindById<Rack>(device.Rack).Parent;
            model.Room = parent.ToString();
            parent = db.FindById<Room>(parent).Parent;
            model.Building = parent.ToString();
            parent = db.FindById<Building>(parent).Parent;
            model.Center = parent.ToString();
            model.City = db.Find<CommCenter>(c => c.Id == parent).Project(c => c.City).FirstOrDefault().ToString();

            ViewBag.Cities = Cities;
            ViewBag.Centers = db.FindGetResults<CommCenter>(c => c.City == ObjectId.Parse(model.City))
                .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id.ToString() == model.Center });
            ViewBag.Buildings = db.FindGetResults<Building>(b => b.Parent == ObjectId.Parse(model.Center))
                .Select(b => new SelectListItem { Text = b.Name, Value = b.Id.ToString(), Selected = b.Id.ToString() == model.Building });
            ViewBag.Rooms = db.FindGetResults<Room>(r => r.Parent == ObjectId.Parse(model.Building))
                .Select(r => new SelectListItem { Text = r.Name, Value = r.Id.ToString(), Selected = r.Id.ToString() == model.Room });
            ViewBag.Racks = db.FindGetResults<Rack>(r => r.Parent == ObjectId.Parse(model.Room))
                .Select(r => new SelectListItem { Text = r.Name, Value = r.Id.ToString(), Selected = r.Id.ToString() == model.Rack });
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(DeviceViewModel m)
        {
            ObjectId centerId, buildingId, roomId, rackId;
            if (!ObjectId.TryParse(m.Center, out centerId))
            {
                ViewBag.Cities = Cities;
                ModelState.AddModelError("Center", "مرکز درست انتخاب نشده است.");
                return View(m);
            }
            if (!ObjectId.TryParse(m.Rack, out rackId))
            {
                if (!ObjectId.TryParse(m.Building, out buildingId))
                {
                    var building = new Building { Name = m.Building, Parent = centerId };
                    db.Save(building);
                    buildingId = building.Id;
                }
                if (!ObjectId.TryParse(m.Room, out roomId))
                {
                    var room = new Room { Name = m.Room, Parent = buildingId };
                    db.Save(room);
                    roomId = room.Id;
                }
                if (!ObjectId.TryParse(m.Rack, out rackId))
                {
                    var rack = new Rack { Name = m.Rack, Parent = roomId };
                    db.Save(rack);
                    rackId = rack.Id;
                }
            }
            var device = db.FindById<Device>(m.Id);
            device.InjectFrom(m);
            device.Rack = rackId;
            db.Save(device);
            return RedirectToAction("Item", "Place", new { type = "Rack", id = rackId.ToString() });
        }
    }
}