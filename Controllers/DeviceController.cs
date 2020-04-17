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
        private IEnumerable<City> cities = null;
        public DeviceController(IDbContext db, IEnumerable<City> cities) : base(db) 
        {
            this.cities = cities;
        }
        
        public IActionResult Add()
        {
            ViewBag.Cities = cities;
            return View();
        }

        [HttpPost]
        public IActionResult Add(DeviceViewModel m)
        {
            ObjectId centerId, buildingId, roomId, rackId;
            if (!ObjectId.TryParse(m.Center, out centerId))
            {
                ViewBag.Cities = cities;
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

            ViewBag.Cities = cities;
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
                ViewBag.Cities = cities;
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

        [AcceptVerbs("GET", "POST")]
        public IActionResult DeviceAddressIsValid(string id, string address)
        {
            ObjectId.TryParse(id, out ObjectId objId);
            bool exists = db.Any<Device>(d => d.Id != objId && d.Address == address);
            return Json(!exists);
        }

    }
}