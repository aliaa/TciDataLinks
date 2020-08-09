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
            return View(new PlaceIndexViewModel { Centers = centers });
        }

        public IActionResult DeviceSearch(Device.DeviceType? type, Device.NetworkType? network, string model, string address)
        {
            var fb = Builders<Device>.Filter;
            var filters = new List<FilterDefinition<Device>>();
            if (type != null)
                filters.Add(fb.Eq(d => d.Type, type.Value));
            if (network != null)
                filters.Add(fb.Eq(d => d.Network, network.Value));
            if (!string.IsNullOrWhiteSpace(model))
                filters.Add(fb.Regex(d => d.Model, new BsonRegularExpression(model.Replace(" ", ".*"))));
            if (!string.IsNullOrWhiteSpace(address))
                filters.Add(fb.Regex(d => d.Address, new BsonRegularExpression(address.Replace(" ", ".*"))));

            var filter = fb.Empty;
            if (filters.Count == 1)
                filter = filters[0];
            else if (filters.Count > 1)
                filter = fb.And(filters);

            var result = db.Find(filter).Limit(20).ToEnumerable().Select(d => Mapper.Map<DeviceViewModel>(d)).ToList();
            if (UserPermissions.Contains(Permission.ViewUserLogs))
            {
                var users = db.All<AuthUserX>().ToDictionary(u => u.Username, u => u.DisplayName);
                foreach (var d in result)
                {
                    d.Logs = db.Find<UserActivity>(a => a.ObjId == d.Id).SortBy(a => a.Time)
                        .Project(a => new UserActivityViewModel
                        {
                            User = users.ContainsKey(a.Username) ? users[a.Username] : a.Username,
                            Time = a.Time,
                            ActivityType = a.ActivityType,
                            ObjId = a.ObjId
                        })
                        .ToList();
                }
            }
            return View(nameof(Index), new PlaceIndexViewModel { DeviceSearchResult = result });
        }

        public IActionResult PassiveSearch(Passive.PassiveTypeEnum? type, Passive.PatchPanelTypeEnum? patchPanelType,
            TransmissionSystemType? transmissionType, string name)
        {
            var fb = Builders<Passive>.Filter;
            var filters = new List<FilterDefinition<Passive>>();
            if (type != null)
                filters.Add(fb.Eq(p => p.Type, type.Value));
            if (patchPanelType != null)
                filters.Add(fb.Eq(p => p.PatchPanelType, patchPanelType.Value));
            if (transmissionType != null)
                filters.Add(fb.Eq(p => p.TransmissionType, transmissionType.Value));
            if (!string.IsNullOrWhiteSpace(name))
                filters.Add(fb.Regex(p => p.Name, new BsonRegularExpression(name.Replace(" ", ".*"))));

            var filter = fb.Empty;
            if (filters.Count == 1)
                filter = filters[0];
            else if (filters.Count > 1)
                filter = fb.And(filters);

            var result = db.Find(filter).Limit(20).ToEnumerable().Select(p => Mapper.Map<PassiveViewModel>(p)).ToList();
            if (UserPermissions.Contains(Permission.ViewUserLogs))
            {
                var users = db.All<AuthUserX>().ToDictionary(u => u.Username, u => u.DisplayName);
                foreach (var d in result)
                {
                    d.Logs = db.Find<UserActivity>(a => a.ObjId == d.Id).SortBy(a => a.Time)
                        .Project(a => new UserActivityViewModel
                        {
                            User = users.ContainsKey(a.Username) ? users[a.Username] : a.Username,
                            Time = a.Time,
                            ActivityType = a.ActivityType,
                            ObjId = a.ObjId
                        })
                        .ToList();
                }
            }
            return View(nameof(Index), new PlaceIndexViewModel { PassiveSearchResult = result });
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
                    model.SubItems = db.Find<Building>(b => b.Parent == objId).SortBy(b => b.Name).ToList();
                    break;
                case PlaceType.Building:
                    model.Building = db.FindById<Building>(objId);
                    model.SubItems = db.Find<Room>(r => r.Parent == objId).SortBy(r => r.Name).ToList();
                    break;
                case PlaceType.Room:
                    model.Room = db.FindById<Room>(objId);
                    model.SubItems = db.Find<Rack>(r => r.Parent == objId).SortBy(r => r.Line).ThenBy(r => r.Index).ThenBy(r => r.Side).ToList();
                    model.NonNetworkItems = db.Find<NonNetworkItem>(x => x.Place == objId).ToList();
                    break;
                case PlaceType.Rack:
                    model.Rack = db.FindById<Rack>(objId);
                    model.SubItems = db.FindGetResults<Device>(d => d.Rack == objId)
                        .Select(d => new PlaceBase(PlaceType.Device) { Id = d.Id, Name = d.ToString(), Parent = objId })
                        .Concat(db.FindGetResults<Passive>(p => p.Rack == objId)
                            .Select(p => new PlaceBase(PlaceType.Passive) { Id = p.Id, Name = p.Name, Parent = objId })).ToList();
                    model.NonNetworkItems = db.Find<NonNetworkItem>(x => x.Place == objId).ToList();
                    break;
                case PlaceType.Device:
                    return RedirectToAction("Edit", "Device", new { id });
                case PlaceType.Passive:
                    return RedirectToAction("Edit", "Passive", new { id });
                default:
                    throw new NotImplementedException();
            }
            if (model.Rack != null && model.Room == null)
                model.Room = db.FindById<Room>(model.Rack.Parent);
            if (model.Room != null && model.Building == null)
                model.Building = db.FindById<Building>(model.Room.Parent);
            if (model.Building != null && model.Center == null)
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
                    if (!db.Any<Room>(r => r.Parent == objId))
                        deleted = db.DeleteOne<Building>(objId).DeletedCount > 0;
                    break;
                case PlaceType.Room:
                    if (!db.Any<Rack>(r => r.Parent == objId))
                        deleted = db.DeleteOne<Room>(objId).DeletedCount > 0;
                    break;
                case PlaceType.Rack:
                    if (!db.Any<Device>(d => d.Rack == objId) && !db.Any<Passive>(p => p.Rack == objId))
                        deleted = db.DeleteOne<Rack>(objId).DeletedCount > 0;
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (deleted)
                return RedirectToAction("Index");
            return RedirectToAction("Item", new { type, id });
        }

        public IActionResult Centers(string city, bool onlyUsed = false)
        {
            var centers = db.Find<CommCenter>(c => c.City == ObjectId.Parse(city))
                .Project(c => new { c.Id, c.Name })
                .SortBy(c => c.Name).ToEnumerable()
                .Select(c => new { c.Id, c.Name });
            if (onlyUsed)
            {
                var usedCenters = db.Aggregate<Building>()
                    .Group(key => key.Parent, g => new { g.Key })
                    .ToEnumerable()
                    .Select(x => x.Key).ToHashSet();
                centers = centers.Where(c => usedCenters.Contains(c.Id));
            }
            return Json(centers.Select(c => new { id = c.Id.ToString(), text = c.Name }));
        }

        public IActionResult Buildings(string center)
        {
            if (ObjectId.TryParse(center, out ObjectId id))
            {
                var buildings = db.FindGetResults<Building>(b => b.Parent == id)
                    .Select(r => new { id = r.Id.ToString(), text = r.ToString() });
                return Json(buildings);
            }
            return Json(Enumerable.Empty<object>());
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

        public IActionResult Passives(string rack)
        {
            if (ObjectId.TryParse(rack, out ObjectId rackId))
            {
                var passives = db.FindGetResults<Passive>(p => p.Rack == rackId)
                    .Select(p => new { id = p.Id.ToString(), text = p.ToString() });
                return Json(passives);
            }
            return Json(Enumerable.Empty<object>());
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        public IActionResult AddRack(Rack rack)
        {
            if (!db.Any<Rack>(r => r.Parent == rack.Parent && r.Line == rack.Line && r.Index == rack.Index && r.Side == rack.Side))
                db.Save(rack);
            return RedirectToAction(nameof(Item), new { type = nameof(PlaceType.Room), id = rack.Parent });
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        [HttpPost]
        public IActionResult Rename([FromForm] ObjectId id, [FromForm] PlaceType type, [FromForm] string name)
        {
            if (type == PlaceType.Building)
                db.UpdateOne<Building>(b => b.Id == id, Builders<Building>.Update.Set(b => b.Name, name));
            else if (type == PlaceType.Room)
                db.UpdateOne<Room>(r => r.Id == id, Builders<Room>.Update.Set(r => r.Name, name));
            else
                throw new NotImplementedException();
            return RedirectToAction(nameof(Item), new { type, id });
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        [HttpPost]
        public IActionResult NewNonNetworkItem(ObjectId placeId, PlaceType placeType, 
             string name, string type, int count)
        {
            NonNetworkItem item;
            if (placeType == PlaceType.Room)
                item = new NonNetworkRoomItem { Type = Enum.Parse<NonNetworkRoomItem.NonNetworkRoomItemType>(type) };
            else if (placeType == PlaceType.Rack)
                item = new NonNetworkRackItem { Type = Enum.Parse<NonNetworkRackItem.NonNetworkRackItemType>(type) };
            else
                throw new NotImplementedException();

            item.Place = placeId;
            item.Name = name;
            item.Count = count;

            db.Save(item);
            return RedirectToAction(nameof(Item), new { type = placeType.ToString(), id = placeId.ToString() });
        }

        [Authorize(nameof(Permission.EditPlacesAndDevices))]
        public IActionResult DeleteNonNetworkItem(ObjectId id, bool isRack)
        {
            var item = db.FindById<NonNetworkItem>(id);
            if (item == null)
                return null;
            db.DeleteOne(item);
            return RedirectToAction(nameof(Item), new { id = item.Place.ToString(), type = isRack ? nameof(PlaceType.Rack) : nameof(PlaceType.Room) });
        }
    }
}