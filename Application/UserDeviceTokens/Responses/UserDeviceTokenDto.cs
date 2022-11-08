using Application.Common.Mappings;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserDeviceTokens.Responses
{
    public class UserDeviceTokenDto: IMapFrom<UserDeviceToken>
    {
        public string userId { get; set; }
        public string deviceToken { get; set; }
        public ApplicationUser user { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UserDeviceToken, UserDeviceTokenDto>().ReverseMap();
        }
    }
}
