using AliaaCommon;
using EasyMongoNet;
using MongoDB.Bson;
using System;
using System.Text;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{
    public class PassiveConnectionViewModel : PassiveConnection
    {
        public ObjectId EndPointId { get; set; }
        public int EndPointIndex { get; set; }
        public int Index { get; set; }

        public string GetPlaceDisplayName(IReadOnlyDbContext db)
        {
            var passive = db.FindById<Passive>(PatchPanel);
            var rack = db.FindById<Rack>(passive.Rack);
            var room = db.FindById<Room>(rack.Parent);

            StringBuilder sb = new StringBuilder();
            sb.Append("اتاق/سالن ").Append(room.Name).Append(" &lArr; ")
                .Append("راک ").Append(rack.Name).Append(" &lArr; ");
            if (passive.Type == Passive.PassiveTypeEnum.PatchPanel)
                sb.Append("پچ پنل ");
            else if (passive.Type == Passive.PassiveTypeEnum.Transmissional)
                sb.Append("تجهیز انتقال ")
                    .Append(DisplayUtils.DisplayName(passive.TransmissionType))
                    .Append(" ");
            else
                throw new NotImplementedException();
            sb.Append(passive.Name);

            return sb.ToString();
        }
    }
}
