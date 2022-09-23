using Application.Categories.Response;
using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Reservations.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Core.Enums;
using Application.Tables.Response;
using Microsoft.AspNetCore.Identity;

namespace Application.Reservations.Commands
{
    public class CreateReservationCommand : IMapFrom<Reservation>, IRequest<Response<ReservationDto>>
    {
        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string UserId { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        public int NumOfSeats { get; set; }
        [Required]
        public TableType tableType { get; set; }
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
            var entity = _mapper.Map<Reservation>(request);

            entity.Status = ReservationStatus.Reserved;

            var TableList = await _unitOfWork.TableRepository.GetTableOnNumOfSeatAndType(request.NumOfSeats, request.tableType);
            int TableId;
            List<int> tableIds = TableList.Select(e => e.Id).ToList();
            if (tableIds.Any())
            {
                TableId = await _unitOfWork.ReservationRepository.GetTableAvailableForReservation(tableIds, request.StartTime, request.EndTime);
                if (TableId == 0)
                {
                    return new Response<ReservationDto>("There is no table available");
                }
            }
            else
            {
                return new Response<ReservationDto>("There is no table available");
            }

            entity.TableId = TableId;
            var result = await _unitOfWork.ReservationRepository.InsertAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<ReservationDto>("error");
            }
            var mappedResult = _mapper.Map<ReservationDto>(result);
            return new Response<ReservationDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
