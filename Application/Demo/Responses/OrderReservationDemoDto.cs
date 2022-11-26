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
        public string Error { get; set; }
    }
}
