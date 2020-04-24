using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TciDataLinks.Models
{
    public class PassiveConnection
    {
        [Display(Name = "پچ پنل")]
        public ObjectId PatchPanel { get; set; }

        [Required(ErrorMessage = "شماره پورت اجباریست")]
        [Remote("PortNumberIsValid", "Connection", AdditionalFields = "PatchPanel", ErrorMessage = "شماره پورت در این پچ پنل قبلا استفاده شده است!")]
        [Display(Name = "شماره پورت")]
        public string PortNumber { get; set; }

        [BsonRepresentation(BsonType.String)]
        [Display(Name = "نوع کانکتور")]
        public EndPoint.ConnectorType Connector { get; set; }

        [Display(Name = "فاصله تا اتصال بعدی (متر)")]
        [Required(ErrorMessage = "فاصله اجباریست")]
        public int DistanceToNextPoint { get; set; }
    }
}
