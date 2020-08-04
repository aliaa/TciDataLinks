using System;
using System.Collections.Generic;
using System.Linq;
using EasyMongoNet;
using EasyMongoNet.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using TciDataLinks.Models;
using TciDataLinks.ViewModels;

namespace TciDataLinks.Controllers
{
    public class LogController : BaseController
    {
        public LogController(IDbContext db) : base(db) { }

        [Authorize(nameof(Permission.ViewUserLogs))]
        public IActionResult Index(string user = "all", string type = nameof(Device))
        {
            var users = db.All<AuthUserX>().ToDictionary(u => u.Username, u => u.DisplayName);

            var fb = Builders<UserActivity>.Filter;
            var filters = new List<FilterDefinition<UserActivity>>();
            if (user != "all")
                filters.Add(fb.Eq(a => a.Username, user));
            if (type == "Link")
                filters.Add(fb.In(a => a.CollectionName, new string[] { nameof(Connection), nameof(EndPoint) }));
            else
                filters.Add(fb.Eq(a => a.CollectionName, type));
            var result = db.Find<UserActivity>(fb.And(filters))
                .Project(a => new UserActivityViewModel
                {
                    User = users.ContainsKey(a.Username) ? users[a.Username] : a.Username,
                    Time = a.Time,
                    ActivityType = a.ActivityType,
                    Type = a.CollectionName,
                    ObjId = a.ObjId
                })
                .SortByDescending(a => a.Time)
                .Limit(1000).ToList();
            foreach (var item in result)
            {
                if (item.Type == nameof(EndPoint))
                {
                    item.Type = nameof(Connection);
                    if (item.ActivityType != ActivityType.Delete)
                        item.ObjId = db.FindById<EndPoint>(item.ObjId)?.Connection ?? ObjectId.Empty;
                }
            }

            ViewBag.User = user;
            ViewBag.Type = type;
            return View(result);
        }
    }
}
