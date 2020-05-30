using EasyMongoNet;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TciDataLinks.Models;

namespace TciDataLinks.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IDbContext db;

        public BaseController(IDbContext db)
        {
            this.db = db;
            db.GetUserNameFunc = () => UserName;
        }

        protected ObjectId? UserId
        {
            get
            {
                ObjectId val;
                if (ObjectId.TryParse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, out val))
                    return val;
                return null;
            }
        }

        protected string UserName => HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        protected IEnumerable<Permission> UserPermissions
        {
            get
            {
                if (User == null)
                    return Enumerable.Empty<Permission>();
                Claim claim = User.Claims.FirstOrDefault(c => c.Type == nameof(Permission));
                if (claim == null)
                    return Enumerable.Empty<Permission>();
                return claim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => (Permission)Enum.Parse(typeof(Permission), c));
            }
        }

        protected AuthUserX GetUser()
        {
            var id = UserId;
            if (id != null)
                return db.FindById<AuthUserX>(id.Value);
            return null;
        }

        protected IActionResult GetEditorTemplatePartialView<T>(T model)
        {
            return PartialView("EditorTemplates/" + typeof(T).Name, model);
        }
    }
}
