using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Demo.Responses
{
    public class OrderReservationDemoDto
    {
        public List<string> created { get; set; }
        public List<string> updated { get; set; }
        public List<string> Error { get; set; }
        public string TotalDishAdded { get; set; }
    }
}
