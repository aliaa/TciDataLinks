using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TciDataLinks.Models
{
    [BsonIgnoreExtraElements]
    public class Customer : MongoEntity
    {
        public enum CustomerServiceType
        {
            IP, Intranet, MPLS, PTP, PTMP, VoIP, VPLS, Special_MPLS, Floating_Internet, Other, unknown // TODO: remove unknown
        }

        [Display(Name = "جمع آوری شده؟")]
        public bool IsAborted { get; set; }

        [Display(Name = "نام مشتری")]
        public string CustomerName { get; set; }

        [Display(Name = "شهر")]
        public ObjectId City { get; set; }

        [Display(Name = "مرکز مخابراتی")]
        public ObjectId Center { get; set; }

        [Display(Name = "شماره پرونده")]
        public long DocumentNumber { get; set; }

        [Display(Name = "شماره دیتا")]
        public long DataNumber { get; set; }

        [Display(Name = "نوع سرویس")]
        [BsonRepresentation(BsonType.String)]
        public CustomerServiceType ServiceType { get; set; }

        //[Display(Name = "نوع سیستم انتقال")]
        //[BsonRepresentation(BsonType.String)]
        //public Device.NetworkType? TransferSystemType { get; set; }

        public override string ToString() => DocumentNumber + " : " + CustomerName;
    }
}
