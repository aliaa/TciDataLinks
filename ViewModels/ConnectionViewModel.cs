using System.Collections.Generic;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{
    public class ConnectionViewModel : Connection
    {
        public List<EndPointViewModel> EndPoints { get; set; } = new List<EndPointViewModel>();
    }
}
