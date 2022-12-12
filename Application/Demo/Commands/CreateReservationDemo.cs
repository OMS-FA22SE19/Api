using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Demo.Response;
using Application.Models;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace Application.Reservations.Commands
{
    public class CreateReservationDemo : IMapFrom<Reservation>, IRequest<Response<ReservationDemoDto>>
    {
        [Required]
        public string StartTime { get; set; }
        public string? EndTime { get; set; }
        public int? numOfCheckInReservation { get; set; }
        public int? numOfAvailableReservation { get; set; }
        public int? numOfCancelledReservation { get; set; }
    }

    public class CreateReservationDemoHandler : IRequestHandler<CreateReservationDemo, Response<ReservationDemoDto>>
    {
        static Random rnd = new Random();
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateReservationDemoHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<Response<ReservationDemoDto>> Handle(CreateReservationDemo request, CancellationToken cancellationToken)
        {
            var startTime = Convert.ToDateTime(request.StartTime);
            DateTime endTime;
            if (!string.IsNullOrWhiteSpace(request.EndTime))
            {
                endTime = Convert.ToDateTime(request.EndTime);
                if (startTime >= endTime)
                {
                    throw new BadRequestException("EndTime is earlier than StartTime");
                }
            }
            else
            {
                endTime = startTime.AddHours(1);
            }

            var users = _userManager.Users.ToList();
            var tables = await _unitOfWork.TableRepository.GetAllAsync();
            var tableTypes = await _unitOfWork.TableTypeRepository.GetAllAsync();

            var ReservationDemoDTO = new ReservationDemoDto()
            {
                ReservationCheckIn = new List<int>(),
                ReservationReserved = new List<int>(),
                ReservationAvailable = new List<int>(),
                ReservationCancelled = new List<int>(),
                Error = ""
            };

            //checkin
            var tableForCheckIn = tables.ToList();
            for (int i = 0; i < request.numOfCheckInReservation; i++)
            {
                if (tableForCheckIn.Count == 0)
                {
                    ReservationDemoDTO.Error = $"Can not add {request.numOfCheckInReservation - i} check in Reservation because there are not enough available table";
                    break;
                }

                int r = rnd.Next(tableForCheckIn.Count());
                var table = tableForCheckIn[r];

                var reservation = new Reservation
                {
                    UserId = users[rnd.Next(users.Count())].Id,
                    NumOfPeople = table.NumOfSeats,
                    NumOfSeats = table.NumOfSeats,
                    TableTypeId = table.TableTypeId,
                    Quantity = 1,
                    StartTime = startTime,
                    EndTime = endTime, //add day +1
                    Status = ReservationStatus.CheckIn,
                    FullName = "Demo",
                    PhoneNumber = "Demo",
                    ReservationTables = new List<ReservationTable>()
                };

                bool isValid = await validateStartEndTime(reservation);
                if (!isValid)
                {
                    i--;
                    tableForCheckIn.Remove(table);
                }

                else
                {
                    reservation.ReservationTables.Add(new ReservationTable()
                    {
                        TableId = table.Id,
                    });

                    var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == table.TableTypeId && !e.IsDeleted);
                    if (tableType is null)
                    {
                        throw new NotFoundException(nameof(TableType), table.TableTypeId);
                    }

                    var result = await _unitOfWork.ReservationRepository.InsertAsync(reservation);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    if (result is null)
                    {
                        return new Response<ReservationDemoDto>("error");
                    }

                    var billing = new Billing
                    {
                        Id = "Demo-" + result.Id,
                        ReservationAmount = table.NumOfSeats * tableType.ChargePerSeat,
                        ReservationId = result.Id
                    };
                    await _unitOfWork.BillingRepository.InsertAsync(billing);
                    await _unitOfWork.CompleteAsync(cancellationToken);

                    ReservationDemoDTO.ReservationCheckIn.Add(result.Id);
                    Console.WriteLine(result.Id);
                    tableForCheckIn.Remove(table);
                }
            }

            //available
            for (int i = 0; i < request.numOfAvailableReservation; i++)
            {
                int r = rnd.Next(tables.Count());
                var table = tables[r];

                int time = rnd.Next(10, 20);
                var availableStartTime = startTime.Date.AddHours(time);
                var availableEndTime = availableStartTime.AddHours(1);

                var reservation = new Reservation
                {
                    UserId = users[rnd.Next(users.Count())].Id,
                    NumOfPeople = table.NumOfSeats,
                    NumOfSeats = table.NumOfSeats,
                    TableTypeId = table.TableTypeId,
                    Quantity = 1,
                    StartTime = availableStartTime,
                    EndTime = availableEndTime,
                    Status = ReservationStatus.Available,
                    FullName = "Demo",
                    PhoneNumber = "Demo",
                    ReservationTables = new List<ReservationTable>()
                };

                await validTime(reservation);

                var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == table.TableTypeId && !e.IsDeleted);
                if (tableType is null)
                {
                    throw new NotFoundException(nameof(TableType), table.TableTypeId);
                }

                var result = await _unitOfWork.ReservationRepository.InsertAsync(reservation);
                await _unitOfWork.CompleteAsync(cancellationToken);
                if (result is null)
                {
                    return new Response<ReservationDemoDto>("error");
                }

                ReservationDemoDTO.ReservationAvailable.Add(result.Id);
                Console.WriteLine(result.Id);
            }

            //Cancalled
            for (int i = 0; i < request.numOfCancelledReservation; i++)
            {
                int r = rnd.Next(tables.Count());
                var table = tables[r];

                int time = rnd.Next(10, 20);
                var cancelStartTime = startTime.Date.AddHours(time);
                var cancelEndTime = cancelStartTime.AddHours(1);

                var reservation = new Reservation
                {
                    UserId = users[rnd.Next(users.Count())].Id,
                    NumOfPeople = table.NumOfSeats,
                    NumOfSeats = table.NumOfSeats,
                    TableTypeId = table.TableTypeId,
                    Quantity = 1,
                    StartTime = cancelStartTime,
                    EndTime = cancelEndTime,
                    Status = ReservationStatus.Cancelled,
                    FullName = "Demo",
                    PhoneNumber = "Demo",
                    ReservationTables = new List<ReservationTable>()
                };

                var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == table.TableTypeId && !e.IsDeleted);
                if (tableType is null)
                {
                    throw new NotFoundException(nameof(TableType), table.TableTypeId);
                }

                var result = await _unitOfWork.ReservationRepository.InsertAsync(reservation);
                await _unitOfWork.CompleteAsync(cancellationToken);
                if (result is null)
                {
                    return new Response<ReservationDemoDto>("error");
                }

                ReservationDemoDTO.ReservationCancelled.Add(result.Id);
                Console.WriteLine(result.Id);
            }

            return new Response<ReservationDemoDto>(ReservationDemoDTO)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }

        private async Task<bool> validateStartEndTime(Reservation request)
        {
            var reservation = await _unitOfWork.ReservationRepository.GetAllReservationWithDate(request.StartTime.Date, request.TableTypeId, request.NumOfSeats);
            double availablePercentage = 1;
            var reservationTables = await _unitOfWork.AdminSettingRepository.GetAsync(e => e.Name.Equals("ReservationTable"));
            if (reservationTables is not null)
            {
                availablePercentage = double.Parse(reservationTables.Value) / 100;
            }
            var tables = await _unitOfWork.TableRepository.GetTableOnNumOfSeatAndType(request.NumOfSeats, request.TableTypeId);
            var maxTables = tables.Count * availablePercentage - request.Quantity;
            var times = reservation.Select(e => e.StartTime).Concat(reservation.Select(e => e.EndTime.AddMinutes(15))).ToImmutableSortedSet();
            var busyTimes = new List<BusyTimeDto>();

            for (int i = 0; i < times.Count - 1; i++)
            {
                var count = reservation.Where(e => e.StartTime <= times[i] && e.EndTime.AddMinutes(15) >= times[i + 1]).ToList().Sum(e => e.Quantity);
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
            return busyTimes.All(e => request.StartTime > e.EndTime || request.EndTime < e.StartTime);
        }

        private async Task validTime(Reservation request)
        {
            bool isValid = await validateStartEndTime(request);
            if (!isValid)
            {
                int time = rnd.Next(10, 20);
                request.StartTime = DateTime.UtcNow.Date.AddHours(time);
                request.EndTime = request.StartTime.AddHours(1);
                await validTime(request);
            }
        }
    }
}
