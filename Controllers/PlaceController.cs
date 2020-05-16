using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyMongoNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using TciCommon.Models;
using TciDataLinks.Models;
using TciDataLinks.ViewModels;

namespace TciDataLinks.Controllers
{
    [Authorize]
    public class PlaceController : BaseController
    {
        private IEnumerable<City> cities = null;

        public PlaceController(IDbContext db, IEnumerable<City> cities) : base(db) 
        {
            this.cities = cities;
        }

        public IActionResult Index()
        {
            var citiesDic = cities.ToDictionary(c => c.Id, c => c.Name);
            var usedCenters = db.Find<Building>(_ => true).Project(b => b.Parent).ToList();
            var centers = db.Find<CommCenter>(_ => true)
                //.Project(c => new { c.Id, c.Name, c.City })
                .ToEnumerable()
                .Where(c => citiesDic.ContainsKey(c.City) && usedCenters.Contains(c.Id))
                .GroupBy(c => c.City)
                .Select(g => new { city = citiesDic[g.Key], List = g.ToList() })
                .ToDictionary(k => k.city, v => v.List);
            return View(new CentersViewModel { Centers = centers });
        }

        public IActionResult Item(string type, string id)
        {
            var objId = ObjectId.Parse(id);
            var model = new PlaceViewModel
            {
                Type = (PlaceType)Enum.Parse(typeof(PlaceType), type)
            };
            switch (model.Type)
            {
                case PlaceType.Center:
                    var center = db.FindById<CommCenter>(objId);
                    var city = cities.First(c => c.Id == center.City);
                    model.Center = new PlaceBase(PlaceType.Center) { Id = objId, Name = center.Name };
                    model.City = new PlaceBase(PlaceType.City) { Id = city.Id, Name = city.Name };
                    model.SubItems = db.FindGetResults<Building>(b => b.Parent == objId);
                    break;
                case PlaceType.Building:
                    model.Building = db.FindById<Building>(objId);
                    model.SubItems = db.FindGetResults<Room>(r => r.Parent == objId);
                    break;
                case PlaceType.Room:
                    model.Room = db.FindById<Room>(objId);
                    model.SubItems = db.Find<Rack>(r => r.Parent == objId).SortBy(r => r.Line).ThenBy(r => r.Index).ThenBy(r => r.Side).ToEnumerable();
                    break;
                case PlaceType.Rack:
                    model.Rack = db.FindById<Rack>(objId);
                    model.SubItems = db.FindGetResults<Device>(d => d.Rack == objId)
                        .Select(d => new PlaceBase(PlaceType.Device) { Id = d.Id, Name = d.ToString(), Parent = objId })
                        .Concat(db.FindGetResults<PatchPanel>(p => p.Rack == objId)
                            .Select(p => new PlaceBase(PlaceType.PatchPanel) { Id = p.Id, Name = p.Name, Parent = objId }));
                    break;
                case PlaceType.Device:
                    return RedirectToAction("Edit", "Device", new { id });
                case PlaceType.PatchPanel:
                    return RedirectToAction("Edit", "PatchPanel", new { id });
                default:
                    throw new NotImplementedException();
            }
            if (model.Rack != null && model.Room == null)
                model.Room = db.FindById<Room>(model.Rack.Parent);
            if (model.Room != null && model.Building == null)
                model.Building = db.FindById<Building>(model.Room.Parent);
            if(model.Building != null && model.Center == null)
            {
                var center = db.FindById<CommCenter>(model.Building.Parent);
                var city = cities.First(c => c.Id == center.City);
                model.Center = new PlaceBase(PlaceType.Center) { Id = center.Id, Name = center.Name, Parent = city.Id };
                model.City = new PlaceBase(PlaceType.City) { Id = city.Id, Name = city.Name };
            }
            return View(model);
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        public IActionResult Delete(string type, string id)
        {
            var objId = ObjectId.Parse(id);
            var t = (PlaceType)Enum.Parse(typeof(PlaceType), type);
            bool deleted = false;
            switch (t)
            {
                case PlaceType.Building:
                    if(!db.Any<Room>(r => r.Parent == objId))
                        deleted = db.DeleteOne<Building>(objId).DeletedCount > 0;
                    break;
                case PlaceType.Room:
                    if (!db.Any<Rack>(r => r.Parent == objId))
                        deleted = db.DeleteOne<Room>(objId).DeletedCount > 0;
                    break;
                case PlaceType.Rack:
                    if (!db.Any<Device>(d => d.Rack == objId) && !db.Any<PatchPanel>(p => p.Rack == objId))
                        deleted = db.DeleteOne<Rack>(objId).DeletedCount > 0;
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (deleted)
                return RedirectToAction("Index");
            return RedirectToAction("Item", new { type, id });
        }

        public IActionResult Centers(string city)
        {
            var centers = db.Find<CommCenter>(c => c.City == ObjectId.Parse(city))
                .Project(c => new { c.Id, c.Name }).ToEnumerable()
                .Select(c => new { id = c.Id.ToString(), text = c.Name });
            return Json(centers);
        }

        public IActionResult Buildings(string center)
        {
            var buildings = db.FindGetResults<Building>(b => b.Parent == ObjectId.Parse(center))
                .Select(r => new { id = r.Id.ToString(), text = r.ToString() });
            return Json(buildings);
        }

        public IActionResult Rooms(string building)
        {
            if (ObjectId.TryParse(building, out ObjectId buildingId))
            {
                var rooms = db.FindGetResults<Room>(r => r.Parent == buildingId)
                    .Select(r => new { id = r.Id.ToString(), text = r.ToString() });
                return Json(rooms);
            }
            return Json(Enumerable.Empty<object>());
        }

        public IActionResult Racks(string room)
        {
            if (ObjectId.TryParse(room, out ObjectId roomId))
            {
                var racks = db.FindGetResults<Rack>(r => r.Parent == roomId)
                    .Select(r => new { id = r.Id.ToString(), text = r.ToString() });
                return Json(racks);
            }
            return Json(Enumerable.Empty<object>());
        }

        public IActionResult Devices(string rack)
        {
            if (ObjectId.TryParse(rack, out ObjectId rackId))
            {
                var devices = db.FindGetResults<Device>(d => d.Rack == rackId)
                    .Select(d => new { id = d.Id.ToString(), text = d.ToString() });
                return Json(devices);
            }
            return Json(Enumerable.Empty<object>());
        }

        public IActionResult PatchPanels(string rack)
        {
            if (ObjectId.TryParse(rack, out ObjectId rackId))
            {
                var patchPanels = db.FindGetResults<PatchPanel>(p => p.Rack == rackId)
                    .Select(p => new { id = p.Id.ToString(), text = p.ToString() });
                return Json(patchPanels);
            }
            return Json(Enumerable.Empty<object>());
        }
    }
}