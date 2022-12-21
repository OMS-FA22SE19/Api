using Application.Common.Interfaces;
using Application.Demo.Responses;
using Application.Helpers;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Demo.Commands
{
    public class CreateAvailableReservationDemo : IRequest<Response<ReservationDemoDto>>
    {
        public string? StartTime { get; set; }
        public int? TableTypeId { get; set; }
        public int? NumOfAvailableReservation { get; set; } = 30;
    }

    public class CreateAvailableReservationDemoHandler : IRequestHandler<CreateAvailableReservationDemo, Response<ReservationDemoDto>>
    {
        static Random rnd = new Random();
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDateTime _dateTime;

        public CreateAvailableReservationDemoHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _dateTime = dateTime;
        }

        public async Task<Response<ReservationDemoDto>> Handle(CreateAvailableReservationDemo request, CancellationToken cancellationToken)
        {
            DateTime startTime;
            if (!string.IsNullOrWhiteSpace(request.StartTime))
            {
                startTime = Convert.ToDateTime(request.StartTime);
            }
            else
            {
                var time = rnd.Next(7, 23);
                startTime = _dateTime.Now.Date.AddHours(time);
            }

            List<string> names = new List<string> {"An","Bảo","Chi","Diệp","Minh","Huy","Tân","Tài","Quân","Vy","Hiếu","Tín","Nhi","Hoàng","Vũ","Linh"};

            var users = await _userManager.GetUsersInRoleAsync("Customer");

            var tables = await _unitOfWork.TableRepository.GetAllAsync();

            var ReservationDemoDTO = new ReservationDemoDto()
            {
                ReservationAvailable = new List<int>(),
                Error = new List<string>()
            };

            var tableForAvailable = tables.ToList();
            for (int i = 0; i < request.NumOfAvailableReservation; i++)
            {
                var nameRandom = rnd.Next(names.Count);
                var name = names[nameRandom];
                if (tableForAvailable.Count == 0)
                {
                    ReservationDemoDTO.Error.Add($"Can not add {request.NumOfAvailableReservation - i} check in Reservation because there are not enough available table");
                    break;
                }

                var phone = "093";
                for(int nr = 0; nr < 7; nr++)
                {
                    var randomNumber = rnd.Next(10);
                    phone += randomNumber.ToString();
                }

                int r = rnd.Next(tableForAvailable.Count());
                var table = tableForAvailable[r];

                var reservation = new Reservation
                {
                    UserId = users[rnd.Next(users.Count())].Id,
                    NumOfPeople = table.NumOfSeats,
                    NumOfSeats = table.NumOfSeats,
                    TableTypeId = table.TableTypeId,
                    Quantity = 1,
                    StartTime = startTime,
                    EndTime = startTime.AddHours(1), //add day +1
                    Status = ReservationStatus.Available,
                    FullName = name,
                    PhoneNumber = phone,
                    ReservationTables = new List<ReservationTable>()
                };

                bool isValid = true;
                string errorMessage = string.Empty;

                var adminSettings = await _unitOfWork.AdminSettingRepository.GetAllAsync();

                var reservations = await _unitOfWork.ReservationRepository.GetAllReservationWithDate(startTime.Date, reservation.TableTypeId, reservation.NumOfSeats);
                var tablesTypes = await _unitOfWork.TableRepository.GetTableOnNumOfSeatAndType(reservation.NumOfSeats, reservation.TableTypeId);
                (isValid, errorMessage) = DateTimeHelpers.ValidateStartEndTime(startTime, startTime.AddHours(1), reservation.Quantity, reservations, tablesTypes.Count, adminSettings);

                if (!isValid)
                {
                    i--;
                    tableForAvailable.Remove(table);
                }
                else
                {
                    var result = await _unitOfWork.ReservationRepository.InsertAsync(reservation);
                    await _unitOfWork.CompleteAsync(cancellationToken);

                    ReservationDemoDTO.ReservationAvailable.Add(result.Id);
                    tableForAvailable.Remove(table);
                }
            }

            return new Response<ReservationDemoDto>(ReservationDemoDTO)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }

        //private async Task<bool> validateStartEndTime(Reservation request)
        //{
        //    var reservation = await _unitOfWork.ReservationRepository.GetAllReservationWithDate(request.StartTime.Date, request.TableTypeId, request.NumOfSeats);
        //    double availablePercentage = 1;
        //    var reservationTables = await _unitOfWork.AdminSettingRepository.GetAsync(e => e.Name.Equals("ReservationTable"));
        //    if (reservationTables is not null)
        //    {
        //        availablePercentage = double.Parse(reservationTables.Value) / 100;
        //    }
        //    var tables = await _unitOfWork.TableRepository.GetTableOnNumOfSeatAndType(request.NumOfSeats, request.TableTypeId);
        //    var maxTables = tables.Count * availablePercentage - request.Quantity;
        //    var times = reservation.Select(e => e.StartTime).Concat(reservation.Select(e => e.EndTime.AddMinutes(15))).ToImmutableSortedSet();
        //    var busyTimes = new List<BusyTimeDto>();

        //    for (int i = 0; i < times.Count - 1; i++)
        //    {
        //        var count = reservation.Where(e => e.StartTime <= times[i] && e.EndTime.AddMinutes(15) >= times[i + 1]).ToList().Sum(e => e.Quantity);
        //        if (maxTables - count < 0)
        //        {
        //            var time = busyTimes.FirstOrDefault(e => e.EndTime == times[i]);
        //            if (time is null)
        //            {
        //                busyTimes.Add(new BusyTimeDto
        //                {
        //                    StartTime = times[i],
        //                    EndTime = times[i + 1]
        //                });
        //            }
        //            else
        //            {
        //                time.EndTime = times[i + 1];
        //            }
        //        }
        //    }
        //    return busyTimes.All(e => request.StartTime > e.EndTime || request.EndTime < e.StartTime);
        //}

        //private async Task validTime(Reservation request)
        //{
        //    //bool isValid = await validateStartEndTime(request);
        //    bool isValid = true;
        //    string errorMessage = string.Empty;

        //    var adminSettings = await _unitOfWork.AdminSettingRepository.GetAllAsync();

        //    var reservations = await _unitOfWork.ReservationRepository.GetAllReservationWithDate(request.StartTime.Date, request.TableTypeId, request.NumOfSeats);
        //    var tables = await _unitOfWork.TableRepository.GetTableOnNumOfSeatAndType(request.NumOfSeats, request.TableTypeId);

        //    (isValid, errorMessage) = DateTimeHelpers.ValidateStartEndTime(request.StartTime, request.EndTime, request.Quantity, reservations, tables.Count, adminSettings);
        //    if (!isValid)
        //    {
        //        int time = rnd.Next(10, 20);
        //        request.StartTime = DateTime.UtcNow.Date.AddHours(time);
        //        request.EndTime = request.StartTime.AddHours(1);
        //        await validTime(request);
        //    }
        //}
    }
}
