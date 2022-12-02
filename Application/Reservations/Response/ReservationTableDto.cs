using Application.Common.Mappings;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;

namespace Application.Reservations.Response
{
    public class ReservationTableDto : IMapFrom<ReservationTable>
    {
        //public int ReservationId { get; set; }
        public int TableId { get; set; }
        public TableDto Table { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ReservationTable, ReservationTableDto>().ReverseMap();
        }

        public bool Equals(ReservationTableDto? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.TableId == other.TableId;
        }
    }

    public sealed class ReservationTableComparer : IEqualityComparer<ReservationTableDto>
    {
        public bool Equals(ReservationTableDto x, ReservationTableDto y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.TableId == y.TableId;
        }

        public int GetHashCode(ReservationTableDto ReservationTableDto)
        {
            if (Object.ReferenceEquals(ReservationTableDto, null)) return 0;

            int hashReservationTableTableId = ReservationTableDto.TableId.GetHashCode();


            return hashReservationTableTableId;
        }
    }
}
