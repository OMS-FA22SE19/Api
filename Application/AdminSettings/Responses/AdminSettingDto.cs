using Application.Common.Mappings;
using Application.OrderDetails.Response;
using Application.Orders.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.AdminSettings.Response
{
    public sealed class AdminSettingDto : IMapFrom<AdminSetting>
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<AdminSetting, AdminSettingDto>()
                .ReverseMap();
        }
    }
}
