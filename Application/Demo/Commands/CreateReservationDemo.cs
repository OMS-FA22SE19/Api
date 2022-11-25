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
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace Application.Reservations.Commands
{
    public class CreateReservationDemo : IMapFrom<Reservation>, IRequest<Response<ReservationDemoDto>>
    {
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        public int numOfCheckInReservation { get; set; }
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
            //bool isValid = await validateStartEndTime(request);
            //if (!isValid)
            //{
            //    return new Response<ReservationDto>($"This reservation is unavailable! Please try again!")
            //    {
            //        StatusCode = System.Net.HttpStatusCode.BadRequest
            //    };
            //}

            var users = _userManager.Users.ToList();
            var tables = await _unitOfWork.TableRepository.GetAllAsync();
            var tableTypes = await _unitOfWork.TableTypeRepository.GetAllAsync();

            var ReservationDemoDTO = new ReservationDemoDto()
            {
                ReservationCheckIn = new List<int>(),
                ReservationReserved = new List<int>(),
                ReservationAvailable = new List<int>(),
                ReservationCancelled = new List<int>(),
            };

            //checkin
            var tableForCheckIn = tables.ToList();
            for (int i = 0; i < request.numOfCheckInReservation; i++)
            {
                if (tableForCheckIn.Count == 0)
                {
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
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Status = ReservationStatus.CheckIn,
                    ReservationTables = new List<ReservationTable>()
                };

                bool isValid = await validateStartEndTime(reservation);
                if (!isValid)
                {
                    reservation.Status = ReservationStatus.Cancelled;
                    var cancelledResult = await _unitOfWork.ReservationRepository.InsertAsync(reservation);
                    await _unitOfWork.CompleteAsync(cancellationToken);

                    ReservationDemoDTO.ReservationCancelled.Add(cancelledResult.Id);
                    Console.WriteLine(cancelledResult.Id);
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
            for (int i = 0; i < request.numOfCheckInReservation; i++)
            {
                int r = rnd.Next(tables.Count());
                var table = tables[r];

                var reservation = new Reservation
                {
                    UserId = users[rnd.Next(users.Count())].Id,
                    NumOfPeople = table.NumOfSeats,
                    NumOfSeats = table.NumOfSeats,
                    TableTypeId = table.TableTypeId,
                    Quantity = 1,
                    StartTime = request.StartTime.AddHours(2),
                    EndTime = request.EndTime.AddHours(2),
                    Status = ReservationStatus.Available,
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

                ReservationDemoDTO.ReservationAvailable.Add(result.Id);
                Console.WriteLine(result.Id);
            }

            //Cancalled
            for (int i = 0; i < request.numOfCheckInReservation; i++)
            {
                int r = rnd.Next(tables.Count());
                var table = tables[r];

                int time = rnd.Next(10, 20);
                var cancelStartTime = request.StartTime.Date.AddHours(time);
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

            var tables = await _unitOfWork.TableRepository.GetTableOnNumOfSeatAndType(request.NumOfSeats, request.TableTypeId);
            var maxTables = tables.Count - request.Quantity;
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
    }
}
