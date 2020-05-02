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
        public enum TransmissionSystemType
        {
            [Display(Name = "مشخص نشده")]
            None,
            RTN,
            PTN,
            DWDM,
            [Display(Name = "رادیو WiFi")]
            Radio_WiFi,
            [Display(Name = "فیبر نوری")]
            FiberOptics,
            S200,
            S380,
            IBAS,
            Line_MUX,
            [Display(Name = "رادیو Combo")]
            Radio_Combo,
            [Display(Name = "رادیو Pasolink")]
            Radio_Pasolink,
            SDH,
            PDH
        }

        [Display(Name = "پچ پنل")]
        public ObjectId PatchPanel { get; set; }

        [Required(ErrorMessage = "شماره پورت اجباریست")]
        //[Remote("PortNumberIsValid", "Connection", AdditionalFields = "PatchPanel", ErrorMessage = "شماره پورت در این پچ پنل قبلا استفاده شده است!")]
        [Display(Name = "شماره پورت")]
        public string PortNumber { get; set; }

        [BsonRepresentation(BsonType.String)]
        [Display(Name = "نوع کانکتور")]
        public EndPoint.ConnectorType Connector { get; set; }

        [Display(Name = "فاصله تا اتصال بعدی (متر)")]
        [Required(ErrorMessage = "فاصله اجباریست")]
        public int DistanceToNextPoint { get; set; }

        [BsonRepresentation(BsonType.String)]
        [Display(Name = "نوع سیستم انتقال")]
        public TransmissionSystemType TransmissionSystem { get; set; }
    }
}
