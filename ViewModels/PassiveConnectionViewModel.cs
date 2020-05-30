using EasyMongoNet;
using System.Text;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
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
            sb.Append("اتاق/سالن ").Append(room.Name).Append(" &lArr; ")
                .Append("راک ").Append(rack.Name).Append(" &lArr; ")
                .Append("پچ پنل ").Append(patchPanel.Name);
            return sb.ToString();
        }
    }
}
