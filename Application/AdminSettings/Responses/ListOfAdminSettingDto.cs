using Application.AdminSettings.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.AdminSettings.Responses
{
    public class ListOfAdminSettingDto
    {
        public IList<AdminSettingDto> adminSettings { get; set; }
    }
}
