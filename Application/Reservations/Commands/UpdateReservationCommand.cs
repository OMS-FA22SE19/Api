using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Foods.Commands;
using Application.Common.Models;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace Application.Reservations.Commands
{
    public class UpdateReservationCommand : IMapFrom<Reservation>, IRequest<Response<ReservationDto>>
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        public int NumOfPeople { get; set; }
        [Required]
        public int NumOfSeats { get; set; }
        [Required]
        public int TableTypeId { get; set; }
        [Required]
        public int Quantity { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateReservationCommand, Reservation>();
        }
    }

    public class UpdateReservationCommandHandler : IRequestHandler<UpdateReservationCommand, Response<ReservationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateReservationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<Response<ReservationDto>> Handle(UpdateReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == request.Id);
            if (reservation is null)
            {
                throw new NotFoundException(nameof(Reservation), request.Id);
            }
            if(reservation.NumOfEdits >= 3)
            {
                return new Response<ReservationDto>($"This reservation is can not be edited more!")
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
            var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == request.TableTypeId && !e.IsDeleted);
            if (tableType is null)
            {
                throw new NotFoundException(nameof(TableType), request.TableTypeId);
            }
            bool isValid = await validateStartEndTime(request);
            if (!isValid)
            {
                return new Response<ReservationDto>($"This reservation is unavailable! Please try again!")
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }

            var billing = await _unitOfWork.BillingRepository.GetAsync(b => b.ReservationId == request.Id);
            if (billing is not null)
            {
                if (billing.ReservationAmount < (request.NumOfSeats * tableType.ChargePerSeat))
                {
                    reservation.Status = ReservationStatus.Available;
                }
            }
            MapToEntity(request, reservation);
            reservation.NumOfEdits++;

            var result = await _unitOfWork.ReservationRepository.UpdateAsync(reservation);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<ReservationDto>("error");
            }
            var mappedResult = _mapper.Map<ReservationDto>(result);

            return new Response<ReservationDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }

        private void MapToEntity(UpdateReservationCommand request, Reservation entity)
        {
            entity.StartTime = request.StartTime;
            entity.EndTime = request.EndTime;
            entity.NumOfPeople = request.NumOfPeople;
            entity.NumOfSeats = request.NumOfSeats;
            entity.TableTypeId = request.TableTypeId;
            entity.Quantity= request.Quantity;
        }

        private async Task<bool> validateStartEndTime(UpdateReservationCommand request)
        {
            var reservation = await _unitOfWork.ReservationRepository.GetAllReservationWithDate(request.StartTime.Date, request.TableTypeId, request.NumOfSeats);
            if (reservation.Any())
            {
                var currentReservation = reservation.SingleOrDefault(r => r.Id == request.Id);
                if (currentReservation != null)
                {
                    reservation.Remove(currentReservation);
                }
            }
            

            var tables = await _unitOfWork.TableRepository.GetTableOnNumOfSeatAndType(request.NumOfSeats, request.TableTypeId);
            var maxTables = tables.Count - request.Quantity;
            var times = reservation.Select(e => e.StartTime).Concat(reservation.Select(e => e.EndTime)).ToImmutableSortedSet();
            var busyTimes = new List<BusyTimeDto>();

            for (int i = 0; i < (times.Count - 1); i++)
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
            return busyTimes.All(e => request.StartTime > e.EndTime || request.EndTime < e.StartTime);
        }
    }
}
