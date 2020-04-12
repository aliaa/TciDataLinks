using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyMongoNet;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using TciCommon.Models;
using TciDataLinks.Models;

namespace TciDataLinks.Controllers
{
    public class PlaceController : BaseController
    {
        public PlaceController(IDbContext db, Settings settings) : base(db, settings) { }

        public IActionResult Index()
        {
            var citiesDic = Cities.ToDictionary(c => c.Id, c => c.Name);
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
                    var city = Cities.First(c => c.Id == center.City);
                    model.Center = new PlaceBase { Id = objId, Name = center.Name };
                    model.City = new PlaceBase { Id = city.Id, Name = city.Name };
                    model.SubItems = db.FindGetResults<Building>(b => b.Parent == objId);
                    break;
                case PlaceType.Building:
                    model.Building = db.FindById<Building>(objId);
                    model.SubItems = db.FindGetResults<Room>(r => r.Parent == objId);
                    break;
                case PlaceType.Room:
                    model.Room = db.FindById<Room>(objId);
                    model.SubItems = db.FindGetResults<Rack>(r => r.Parent == objId);
                    break;
                case PlaceType.Rack:
                    model.Rack = db.FindById<Rack>(objId);
                    model.SubItems = db.FindGetResults<Device>(d => d.Rack == objId)
                        .Select(d => new PlaceBase { Id = d.Id, Name = d.ToString(), Parent = objId });
                    break;
                case PlaceType.Device:
                    return RedirectToAction("Item", "Device", new { id });
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
                var city = Cities.First(c => c.Id == center.City);
                model.Center = new PlaceBase { Id = center.Id, Name = center.Name, Parent = city.Id };
                model.City = new PlaceBase { Id = city.Id, Name = city.Name };
            }
            return View(model);
        }

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
                    if (!db.Any<Device>(d => d.Rack == objId))
                        deleted = db.DeleteOne<Rack>(objId).DeletedCount > 0;
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (deleted)
                return RedirectToAction("Index");
            return RedirectToAction("Item", new { type, id });
        }
    }
}