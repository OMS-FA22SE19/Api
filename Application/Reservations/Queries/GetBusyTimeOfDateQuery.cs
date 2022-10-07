using Application.Models;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
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
            var result = await _unitOfWork.ReservationRepository.GetAllReservationWithDate(request.Date);

            var TableList = await _unitOfWork.TableRepository.GetTableOnNumOfSeatAndType(request.NumOfSeats, request.TableTypeId);
            List<int> tableIds = TableList.Select(e => e.Id).ToList();

            List<BusyTimeDto> listOfBusyTimes = new List<BusyTimeDto>();
            if (tableIds.Any())
            {
                foreach (Reservation reservation in result.ToList())
                {
                    if (!tableIds.Contains(reservation.TableId))
                    {
                        result.Remove(reservation);
                    }
                }
                if (tableIds.Count == 1)
                {
                    listOfBusyTimes = _mapper.Map<List<BusyTimeDto>>(result);
                }
                else
                {
                    foreach (int tableId in tableIds)
                    {
                        var listOfBusyTimesForThisTable = _mapper.Map<List<BusyTimeDto>>(result.Where(r => r.TableId == tableId));
                        if (!listOfBusyTimesForThisTable.Any())
                        {
                            listOfBusyTimes = new List<BusyTimeDto>();
                            new Response<List<BusyTimeDto>>(listOfBusyTimes);
                        }
                        if (!listOfBusyTimes.Any())
                        {
                            listOfBusyTimes = listOfBusyTimesForThisTable;
                        }
                        else
                        {
                            int counter = 0;
                            foreach (BusyTimeDto busyTime in listOfBusyTimes.ToList())
                            {
                                if (counter == 0)
                                {
                                    listOfBusyTimes = new List<BusyTimeDto>();
                                }

                                foreach (BusyTimeDto BusyTimeForThisTable in listOfBusyTimesForThisTable)
                                {
                                    if (busyTime.StartTime < BusyTimeForThisTable.StartTime
                                        && busyTime.EndTime < BusyTimeForThisTable.EndTime
                                        && busyTime.EndTime > BusyTimeForThisTable.StartTime)
                                    {
                                        busyTime.StartTime = BusyTimeForThisTable.StartTime;
                                        listOfBusyTimes.Add(busyTime);
                                    }
                                    else
                                    {
                                        if (busyTime.StartTime >= BusyTimeForThisTable.StartTime
                                            && busyTime.StartTime <= BusyTimeForThisTable.EndTime)
                                        {
                                            if (busyTime.EndTime < BusyTimeForThisTable.EndTime)
                                            {
                                                listOfBusyTimes.Add(busyTime);
                                            }
                                            else
                                            {
                                                busyTime.EndTime = BusyTimeForThisTable.EndTime;
                                                listOfBusyTimes.Add(busyTime);
                                            }
                                        }
                                        else
                                        {
                                            listOfBusyTimes.Remove(busyTime);
                                        }
                                    }
                                }
                                counter++;
                            }
                        }
                    }
                }
            }
            return new Response<List<BusyTimeDto>>(listOfBusyTimes);
        }
    }
}
