using System.Collections.Generic;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{
    public class ConnectionViewModel : Connection
    {
        public List<EndPointViewModel> EndPoints { get; set; } = new List<EndPointViewModel>();
        public string CreateDate { get; set; }
        public string CreatedUser { get; set; }
        public string LastEditDate { get; set; }
        public string EditedUser { get; set; }
    }
}
