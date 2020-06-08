using EasyMongoNet;
using Omu.ValueInjecter;
using System.ComponentModel.DataAnnotations;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{
    public class PassiveViewModel : PlaceBasedViewModel
    {
        [Display(Name = "نوع راک")]
        public Rack.RackType RackType { get; set; }

        [Display(Name = "نوع رابط")]
        public Passive.PassiveTypeEnum Type { get; set; }

        [Display(Name = "نوع پچ پنل")]
        public Passive.PatchPanelTypeEnum PatchPanelType { get; set; }

        [Display(Name = "نوع تجهیزات انتقال")]
        public PassiveConnection.TransmissionSystemType TransmissionType { get; set; }

        [Required(ErrorMessage = "نام اجباریست!")]
        [Display(Name = "نام")]
        public string Name { get; set; }

        public override string ToString() => Name;

        public string GetPlaceDisplay(IReadOnlyDbContext db)
        {
            return Mapper.Map<Passive>(this).GetPlaceDisplay(db);
        }
    }
}
