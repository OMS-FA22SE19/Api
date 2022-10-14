using Application.Models;
using Application.Reservations.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace Application.Reservations.Queries
{
    public class GetBusyTimeOfDateQuery : IRequest<Response<List<BusyTimeDto>>>
    {
        [Required]
        public DateTime Date { get; init; }
        [Required]
        public int NumOfSeats { get; set; }
        [Required]
        public int TableTypeId { get; set; }
        [Required]
        public int Quantity { get; set; }
    }

    public class GetBusyTimeOfDateQueryHandler : IRequestHandler<GetBusyTimeOfDateQuery, Response<List<BusyTimeDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetBusyTimeOfDateQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<List<BusyTimeDto>>> Handle(GetBusyTimeOfDateQuery request, CancellationToken cancellationToken)
        {
            var reservation = await _unitOfWork.ReservationRepository.GetAllReservationWithDate(request.Date, request.TableTypeId, request.NumOfSeats);

            var tables = await _unitOfWork.TableRepository.GetTableOnNumOfSeatAndType(request.NumOfSeats, request.TableTypeId);
            var maxTables = tables.Count - request.Quantity;
            var times = reservation.Select(e => e.StartTime).Concat(reservation.Select(e => e.EndTime)).ToImmutableSortedSet();
            var busyTimes = new List<BusyTimeDto>();

            for (int i = 0; i < times.Count - 1; i++)
            {
                var count = reservation.Where(e => e.StartTime <= times[i] && e.EndTime >= times[i + 1]).ToList().Sum(e => e.Quantity);
                if (maxTables - count < 0)
                {
                    var time = busyTimes.FirstOrDefault(e => e.EndTime == times[i]);
                    if (time is null)
                    {
                        busyTimes.Add(new BusyTimeDto
                        {
                            StartTime = times[i],
                            EndTime = times[i + 1]
                        });
                    }
                    else
                    {
                        time.EndTime = times[i + 1];
                    }
                }
            }

            return new Response<List<BusyTimeDto>>(busyTimes);
        }
    }
}
