﻿using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using TciDataLinks.Controllers;

namespace TciDataLinks.Models
{
    public class PassiveConnection
    {
        [Display(Name = "رابط Passive")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PatchPanel { get; set; }

        [Required(ErrorMessage = "شماره پورت اجباریست")]
        //[Remote(nameof(ConnectionController.PassivePortIsValid), "Connection", AdditionalFields = nameof(PatchPanel) + ",EndPointId", ErrorMessage = "شماره پورت در این پچ پنل قبلا استفاده شده است!")]
        [Display(Name = "شماره پورت")]
        public string PortNumber { get; set; }

        [BsonRepresentation(BsonType.String)]
        [Display(Name = "نوع کانکتور")]
        public EndPoint.ConnectorType Connector { get; set; }

        [Display(Name = "اتصال تا رابط بعدی پرزوج است")]
        public bool ConnectionIsBulk { get; set; }

        [Display(Name = "فاصله تا اتصال بعدی (متر)")]
        [Required(ErrorMessage = "فاصله اجباریست")]
        public int DistanceToNextPoint { get; set; }
    }
}
