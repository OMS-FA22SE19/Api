using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Events
{
    public class CourseTypeEvent : BaseEvent
    {
        public string message { get; set; }
        public CourseTypeEvent(string message) { this.message = message; }
    }
}
