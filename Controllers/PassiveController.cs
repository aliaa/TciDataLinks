﻿using EasyMongoNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;
using MongoDB.Driver;
using Omu.ValueInjecter;
using System.Collections.Generic;
using System.Linq;
using TciCommon.Models;
using TciDataLinks.Models;
using TciDataLinks.ViewModels;

namespace TciDataLinks.Controllers
{
    [Authorize]
    public class PassiveController : BaseController
    {
        private IEnumerable<City> cities = null;
        public PassiveController(IDbContext db, IEnumerable<City> cities) : base(db)
        {
            this.cities = cities;
        }

        public IActionResult Item(ObjectId id)
        {
            var passive = db.FindById<Passive>(id);
            var model = Mapper.Map<PassiveViewModel>(passive);
            return View(model);
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        public IActionResult Add(ObjectId city, ObjectId center, ObjectId building, ObjectId room, ObjectId rack)
        {
            var model = new PassiveViewModel
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
            if (building != ObjectId.Empty)
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
        public IActionResult Add(PassiveViewModel m)
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
            var rack = db.Find<Rack>(r => r.Parent == roomId && r.Line == m.RackLine && r.Index == m.RackIndex && r.Side == m.RackSide)
                .FirstOrDefault();
            if (rack == null)
            {
                rack = new Rack { Parent = roomId, Line = m.RackLine, Index = m.RackIndex, Type = m.RackType, Side = m.RackSide };
                db.Save(rack);
                rackId = rack.Id;
            }
            else
            {
                rackId = rack.Id;
                if (rack.Type != m.RackType && !db.Any<Device>(d => d.Rack == rack.Id))
                {
                    rack.Type = m.RackType;
                    db.Save(rack);
                }
            }

            var passive = Mapper.Map<Passive>(m);
            passive.Rack = rackId;

            db.Save(passive);
            return RedirectToAction("Item", "Place", new { type = "Rack", id = rackId.ToString() });
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        public IActionResult Edit(string id)
        {
            var passive = db.FindById<Passive>(id);
            var model = Mapper.Map<PassiveViewModel>(passive);
            var rack = db.FindById<Rack>(passive.Rack);
            model.RackType = rack.Type;
            model.RackLine = rack.Line;
            model.RackIndex = rack.Index;
            model.RackSide = rack.Side;
            model.RackRow = passive.RackRow;
            model.Room = rack.Parent.ToString();
            var parent = db.FindById<Room>(rack.Parent).Parent;
            model.Building = parent.ToString();
            parent = db.FindById<Building>(parent).Parent;
            model.Center = parent;
            model.City = db.FindById<CommCenter>(parent).City;

            ViewBag.Cities = cities;
            ViewBag.Centers = db.FindGetResults<CommCenter>(c => c.City == model.City)
                .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == model.Center });
            ViewBag.Buildings = db.FindGetResults<Building>(b => b.Parent == model.Center)
                .Select(b => new SelectListItem { Text = b.Name, Value = b.Id.ToString(), Selected = b.Id.ToString() == model.Building });
            ViewBag.Rooms = db.FindGetResults<Room>(r => r.Parent == ObjectId.Parse(model.Building))
                .Select(r => new SelectListItem { Text = r.Name, Value = r.Id.ToString(), Selected = r.Id.ToString() == model.Room });
            //ViewBag.Racks = db.FindGetResults<Rack>(r => r.Parent == ObjectId.Parse(model.Room))
            //    .Select(r => new SelectListItem { Text = r.Name, Value = r.Id.ToString(), Selected = r.Id.ToString() == model.Rack });
            return View(model);
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        [HttpPost]
        public IActionResult Edit(PassiveViewModel m)
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
            var rack = db.Find<Rack>(r => r.Parent == roomId && r.Line == m.RackLine && r.Index == m.RackIndex && r.Side == m.RackSide)
                .FirstOrDefault();
            if (rack == null)
            {
                rack = new Rack { Parent = roomId, Type = m.RackType, Line = m.RackLine, Index = m.RackIndex, Side = m.RackSide };
                db.Save(rack);
                rackId = rack.Id;
            }
            else
            {
                rackId = rack.Id;
                if (rack.Type != m.RackType && !db.Any<Device>(d => d.Rack == rackId))
                {
                    rack.Type = m.RackType;
                    db.Save(rack);
                }
            }

            var passive = db.FindById<Passive>(m.Id);
            passive.InjectFrom(m);
            passive.Rack = rackId;
            db.Save(passive);
            return RedirectToAction("Item", "Place", new { type = "Rack", id = rackId.ToString() });
        }

        public IActionResult Delete(ObjectId id)
        {
            var pp = db.FindById<Passive>(id);
            if (!db.Any<EndPoint>(e => e.PassiveConnections.Any(p => p.PatchPanel == id)))
                db.DeleteOne<Passive>(id);
            return RedirectToAction("Item", "Place", new { type = "Rack", id = pp.Rack.ToString() });
        }
    }
}