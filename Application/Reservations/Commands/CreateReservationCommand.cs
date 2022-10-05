using Application.Common.Mappings;
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

namespace Application.Reservations.Commands
{
    public class CreateReservationCommand : IMapFrom<Reservation>, IRequest<Response<ReservationDto>>
    {
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        public int NumOfSeats { get; set; }
        [Required]
        public int TableTypeId { get; set; }
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

            var user = await _userManager.Users.FirstOrDefaultAsync(e => e.UserName.Equals("defaultCustomer"), cancellationToken);
            entity.UserId = user.Id;

            entity.Status = ReservationStatus.Reserved;

            var tableList = await _unitOfWork.TableRepository.GetTableOnNumOfSeatAndType(request.NumOfSeats, request.TableTypeId);
            int tableId;
            List<int> tableIds = tableList.Select(e => e.Id).ToList();
            if (tableIds.Any())
            {
                tableId = await _unitOfWork.TableRepository.GetTableAvailableForReservation(tableIds, request.StartTime, request.EndTime);
                if (tableId == 0)
                {
                    return new Response<ReservationDto>("There is no table available");
                }
            }
            else
            {
                return new Response<ReservationDto>("There is no table available");
            }

            entity.TableId = tableId;
            var result = await _unitOfWork.ReservationRepository.InsertAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<ReservationDto>("error");
            }
            result.Table = await _unitOfWork.TableRepository.GetAsync(e => e.Id == tableId && !e.IsDeleted, $"{nameof(Table.TableType)}");
            result.User = user;
            var mappedResult = _mapper.Map<ReservationDto>(result);
            mappedResult.User = user;
            mappedResult.Table = await _unitOfWork.TableRepository.GetAsync(e => e.Id == TableId);
            return new Response<ReservationDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
