using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public sealed class UserDeviceToken: Entity
    {
        [Required]
        public string userId { get; set; }
        [Required]
        public string deviceToken { get; set; }

        public ApplicationUser User { get; set; }
    }
}
