using Application.Common.Exceptions;
using Application.Common.Mappings;
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
    public class CreateReservationCommand : IMapFrom<Reservation>, IRequest<Response<ReservationDto>>
    {
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
        public bool IsPriorFoodOrder { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateReservationCommand, Reservation>();
        }
    }

    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Response<ReservationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateReservationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<Response<ReservationDto>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
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
            var entity = _mapper.Map<Reservation>(request);
            var user = await _userManager.Users.FirstOrDefaultAsync(e => e.UserName.Equals("defaultCustomer"), cancellationToken);
            entity.UserId = user.Id;
            entity.Status = ReservationStatus.Available;
            var result = await _unitOfWork.ReservationRepository.InsertAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<ReservationDto>("error");
            }
            result.User = user;
            var mappedResult = _mapper.Map<ReservationDto>(result);
            mappedResult.PrePaid = entity.NumOfSeats * tableType.ChargePerSeat * entity.Quantity;
            mappedResult.TableType = tableType.Name;
            return new Response<ReservationDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }

        private async Task<bool> validateStartEndTime(CreateReservationCommand request)
        {
            var reservation = await _unitOfWork.ReservationRepository.GetAllReservationWithDate(request.StartTime.Date, request.TableTypeId, request.NumOfSeats);

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
