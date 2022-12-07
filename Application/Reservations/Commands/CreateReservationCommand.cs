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
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        public bool IsPriorFoodOrder { get; set; } = false;

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
        private readonly ICurrentUserService _currentUserService;

        public CreateReservationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<Response<ReservationDto>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
            bool isValid = true;
            string errorMessage = string.Empty;
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

            (isValid, errorMessage) = DateTimeHelpers.ValidateStartEndTime(request.StartTime, request.EndTime, request.Quantity, reservations, tables.Count, adminSettings);
            if (!isValid)
            {
                return new Response<ReservationDto>(errorMessage)
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }

            var entity = _mapper.Map<Reservation>(request);
            ApplicationUser user = null;
            if (_currentUserService.UserId is not null)
            {
                user = await _userManager.FindByIdAsync(_currentUserService.UserId);
            }
            else
            {
                user = await _userManager.Users.FirstOrDefaultAsync(e => e.UserName.Equals("defaultCustomer"));
            }
            
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
    }
}
