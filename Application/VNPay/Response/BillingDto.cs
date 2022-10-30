using Application.Common.Mappings;
using AutoMapper;

namespace Application.Types.Response
{
    public sealed class BillingDto : IMapFrom<Core.Entities.Billing>
    {
        public string Id { get; set; }
        public int ReservationId { get; set; }
        public string Status { get; set; }
        public double Amount { get; set; }
        public string OrderId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Core.Entities.Billing, BillingDto>();
        }
    }
}
