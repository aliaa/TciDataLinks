using EasyMongoNet;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TciDataLinks.Models
{
    public class PassiveConnectionViewModel : PassiveConnection
    {
        public int EndPointIndex { get; set; }
        public int Index { get; set; }

        public string GetPlaceDisplayName(IReadOnlyDbContext db)
        {
            var patchPanel = db.FindById<PatchPanel>(PatchPanel);
            var rack = db.FindById<Rack>(patchPanel.Rack);
            var room = db.FindById<Room>(rack.Parent);
            
            StringBuilder sb = new StringBuilder();
            sb.Append("اتاق/سالن ").Append(room.Name).Append(" > ")
                .Append("راک ").Append(rack.Name).Append(" > ")
                .Append("پچ پنل ").Append(patchPanel.Name);
            return sb.ToString();
        }
    }
}
