using EasyMongoNet;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    [CollectionSave(WriteLog = true)]
    [CollectionIndex(new string[] { nameof(Connection) })]
    [CollectionIndex(new string[] { nameof(Device) })]
    [CollectionIndex(new string[] { nameof(PassiveConnections)+"."+nameof(PassiveConnection.PatchPanel) })]
    public class EndPoint : MongoEntity
    {
        public enum PortTypeEnum
        {
            [Display(Name = "الکتریکال")]
            Electrical,
            [Display(Name = "اپتیکال")]
            Optical
        }

        public enum SpeedUnitEnum
        {
            Kbps,
            Mbps,
            Gbps,
        }

        public enum ModuleType
        {
            [Display(Name = "مشخص نشده")]
            Unspecified,
            SFP,
            [Display(Name = "SFP+")]
            SFP_Plus,
            XFP,
            [Display(Name = "GLC-T")]
            GLC_T,
        }

        public enum PatchCordType
        {
            [Display(Name = "مشخص نشده")]
            None,
            [Display(Name = "Single-Mode")]
            SingleMode,
            [Display(Name = "Multi-Mode")]
            MultiMode
        }

        public enum ConnectorType
        {
            RJ45,
            BNC,
            SMB,
            [Display(Name = "FC/PC")]
            FCPC,
            [Display(Name = "FC/APC")]
            PCAPC,
            [Display(Name = "LC/PC")]
            LCPC,
            [Display(Name = "LC/APC")]
            LCAPC,
            [Display(Name = "SC/PC")]
            SCPC,
            [Display(Name = "SC/APC")]
            SCAPC,
            [Display(Name = "3C2V")]
            _3C2V,
            G62,
        }

        public ObjectId Connection { get; set; }
        public int Index { get; set; }
        public ObjectId Device { get; set; }

        public List<PassiveConnection> PassiveConnections { get; set; } = new List<PassiveConnection>();

        [BsonRepresentation(BsonType.String)]
        [Display(Name = "نوع پورت")]
        public PortTypeEnum PortType { get; set; }

        [Required(ErrorMessage = "شماره پورت اجباریست")]
        //[Remote("PortNumberIsValid", "Connection", AdditionalFields = "Device", ErrorMessage = "شماره پورت در این دستگاه قبلا استفاده شده است!")]
        [Display(Name = "شماره پورت")]
        public string PortNumber { get; set; }
        
        [Display(Name = "ظرفیت لینک")]
        [Required(ErrorMessage = "ظرفیت لینک اجباریست")]
        public int Speed { get; set; }

        [BsonRepresentation(BsonType.String)]
        [Display(Name = "واحد ظرفیت لینک")]
        public SpeedUnitEnum SpeedUnit { get; set; } = SpeedUnitEnum.Gbps;
        
        [BsonRepresentation(BsonType.String)]
        [Display(Name = "نوع ماژول")]
        public ModuleType Module { get; set; }

        [BsonRepresentation(BsonType.String)]
        [Display(Name = "نوع پچ کورد")]
        public PatchCordType PatchCord { get; set; }

        [BsonRepresentation(BsonType.String)]
        [Display(Name = "نوع کانکتور")]
        public ConnectorType Connector { get; set; }

        [Display(Name = "فاصله تا اتصال بعدی (متر)")]
        [Required(ErrorMessage = "فاصله اجباریست")]
        public int DistanceToNextPoint { get; set; }

        [Display(Name = "پروتکشن دیتا")]
        public bool DataProtection { get; set; }

        [Display(Name = "پروتکشن سیستمهای انتقال")]
        public bool TransmissionProtection { get; set; }

        [Display(Name = "اطلاعات اتصال ناقص است")]
        public bool Incomplete { get; set; }

        [Display(Name = "توضیح")]
        public string Description { get; set; }
    }
}
