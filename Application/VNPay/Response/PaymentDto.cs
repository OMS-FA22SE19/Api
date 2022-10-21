using Application.Common.Mappings;
using AutoMapper;

namespace Application.Types.Response
{
    public sealed class PaymentDto : IMapFrom<Core.Entities.Payment>
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string Status { get; set; }
        public double Amount { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Core.Entities.Payment, PaymentDto>();
        }
    }
}
