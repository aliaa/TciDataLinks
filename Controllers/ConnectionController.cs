using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyMongoNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Omu.ValueInjecter;
using TciCommon.Models;
using TciDataLinks.Models;

namespace TciDataLinks.Controllers
{
    [Authorize]
    public class ConnectionController : BaseController
    {
        private IEnumerable<City> cities = null;
        public ConnectionController(IDbContext db, IEnumerable<City> cities) : base(db) 
        {
            this.cities = cities;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add()
        {
            return View(new ConnectionViewModel());
        }

        [HttpPost]
        public IActionResult Add(ConnectionViewModel model)
        {
            var connection = Mapper.Map<Connection>(model);
            foreach (var vm in model.EndPoints)
            {
                var endPoint = Mapper.Map<EndPoint>(vm);
                connection.EndPoints.Add(endPoint);
                foreach (var passive in vm.PassiveConnectionViewModels)
                    endPoint.PassiveConnections.Add(Mapper.Map<PassiveConnection>(passive));
            }
            db.Save(connection);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AddEndPoint(int index, ObjectId device)
        {
            return GetEditorTemplatePartialView<EndPoint>(new EndPointViewModel
            { 
                Index = index, 
                Device = device
            });
        }

        public IActionResult AddPassiveConnection(int endPointIndex, int index, ObjectId patchPanel)
        {
            return GetEditorTemplatePartialView<PassiveConnection>(new PassiveConnectionViewModel
            {
                EndPointIndex = endPointIndex,
                Index = index,
                PatchPanel = patchPanel
            });
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult PortNumberIsValid(string portNumber, ObjectId device, ObjectId patchPanel)
        {
            if (device != ObjectId.Empty)
            {
                var exists = db.Any<Connection>(c => c.EndPoints.Any(e => e.Device == device && e.PortNumber == portNumber));
                return Json(!exists);
            }
            else if (patchPanel != ObjectId.Empty)
            {
                var exists = db.Any<Connection>(c => c.EndPoints.Any(e => e.PassiveConnections.Any(p => p.PatchPanel == patchPanel && p.PortNumber == portNumber)));
                return Json(!exists);
            }
            //TODO
            return Json(true);
        }
    }
}