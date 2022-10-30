using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Models;
using Application.Orders.Events;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Reservations.Commands
{
    public sealed class CheckinReservationCommand : IRequest<Response<ReservationDto>>
    {
    }

    public sealed class CheckinReservationCommandHandler : IRequestHandler<CheckinReservationCommand, Response<ReservationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;

        public CheckinReservationCommandHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _dateTime = dateTime;
        }

        public async Task<Response<ReservationDto>> Handle(CheckinReservationCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(e => e.UserName.Equals("defaultCustomer"), cancellationToken);
            var entity = await _unitOfWork.ReservationRepository.GetAsync(e => e.UserId.Equals(user.Id)
                && _dateTime.Now >= e.StartTime.AddMinutes(-15) && _dateTime.Now <= e.EndTime
                && e.Status == ReservationStatus.Reserved
                && !e.IsDeleted,
                    $"{nameof(Reservation.ReservationTables)}");
            if (entity is null)
            {
                throw new NotFoundException($"No reservation found for user {user.FullName}");
            }

            entity.Status = ReservationStatus.CheckIn;

            List<Expression<Func<Table, bool>>> filters = new();
            filters.Add(e => !e.IsDeleted && e.Status == TableStatus.Available && e.NumOfSeats == entity.NumOfSeats && e.TableTypeId == entity.TableTypeId);
            var tables = await _unitOfWork.TableRepository.GetPaginatedListAsync(filters, pageSize: entity.Quantity);

            var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => !e.IsDeleted && e.Id == entity.TableTypeId);

            var tableIds = new List<int>();

            foreach (var table in tables)
            {
                table.Status = TableStatus.Occupied;

                await _unitOfWork.ReservationTableRepository.InsertAsync(new ReservationTable
                {
                    ReservationId = entity.Id,
                    TableId = table.Id
                });

                tableIds.Add(table.Id);

                await _unitOfWork.TableRepository.UpdateAsync(table);
                table.TableType = tableType;
            }

            var result = await _unitOfWork.ReservationRepository.UpdateAsync(entity);
            entity.AddDomainEvent(new CheckInReservationEvent
            {
                ReservationId = entity.Id,
                tableIds = tableIds,
            });
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<ReservationDto>("error");
            }
            var mappedResult = _mapper.Map<ReservationDto>(result);
            mappedResult.TableType = tableType.Name;
            return new Response<ReservationDto>(mappedResult);
        }
    }
}
