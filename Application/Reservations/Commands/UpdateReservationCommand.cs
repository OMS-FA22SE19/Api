using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Helpers;
using Application.Models;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
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
        private readonly ICurrentUserService _currentUserService;

        public UpdateReservationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<Response<ReservationDto>> Handle(UpdateReservationCommand request, CancellationToken cancellationToken)
        {
            bool isValid = true;
            string errorMessage = string.Empty;

            var reservation = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == request.Id);
            if (reservation is null)
            {
                throw new NotFoundException(nameof(Reservation), request.Id);
            }

            if (_currentUserService.UserId is null)
            {
                throw new BadRequestException("You have to log in");
            }
            if (!_currentUserService.UserName.Equals("defaultCustomer"))
            {
                if (!_currentUserService.UserId.Equals(reservation.UserId))
                {
                    throw new BadRequestException("This is not your reservation");
                }
            }

            var maxEdit = await _unitOfWork.AdminSettingRepository.GetAsync(e => e.Name.Equals("MaxEdit"));
            var editAmount = 0;
            if (maxEdit is not null)
            {
                int.TryParse(maxEdit.Value, out editAmount);
            }
            if (reservation.NumOfEdits >= (editAmount))
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

            var adminSettings = await _unitOfWork.AdminSettingRepository.GetAllAsync();

            (isValid, errorMessage) = DateTimeHelpers.ValidateDateInAvailableTime(request.StartTime, request.EndTime, adminSettings);

            if (!isValid)
            {
                return new Response<ReservationDto>(errorMessage)
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }

            var reservations = await _unitOfWork.ReservationRepository.GetAllReservationWithDate(request.StartTime.Date, request.TableTypeId, request.NumOfSeats);
            var tables = await _unitOfWork.TableRepository.GetTableOnNumOfSeatAndType(request.NumOfSeats, request.TableTypeId);
            bool isDefaultCustomer = (_currentUserService?.Role?.Equals("Customer") != true) || _currentUserService?.UserName?.Equals("DefaultCustomer") == true;
            (isValid, errorMessage) = DateTimeHelpers.ValidateStartEndTime(request.StartTime, request.EndTime, request.Quantity, reservations, tables.Count, adminSettings, isDefaultCustomer);
            if (!isValid)
            {
                return new Response<ReservationDto>(errorMessage)
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }

            var billing = await _unitOfWork.BillingRepository.GetAsync(b => b.ReservationId == request.Id);
            if (billing is not null)
            {
                if (billing.ReservationAmount < (request.NumOfSeats * tableType.ChargePerSeat * request.Quantity))
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
            entity.Quantity = request.Quantity;
        }
    }
}
