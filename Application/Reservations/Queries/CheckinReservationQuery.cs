using Application.Common.Exceptions;
using Application.Common.Interfaces;
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

namespace Application.Reservations.Queries
{
    public sealed class CheckinReservationQuery : IRequest<Response<ReservationDto>>
    {
    }

    public sealed class CheckinReservationQueryHandler : IRequestHandler<CheckinReservationQuery, Response<ReservationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;

        public CheckinReservationQueryHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _dateTime = dateTime;
        }

        public async Task<Response<ReservationDto>> Handle(CheckinReservationQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(e => e.UserName.Equals("defaultCustomer"), cancellationToken);
            var entity = await _unitOfWork.ReservationRepository.GetAsync(e => e.UserId.Equals(user.Id) && _dateTime.Now >= e.StartTime.AddMinutes(-15) && _dateTime.Now <= e.EndTime,
                $"{nameof(Reservation.ReservationTables)}");
            if (entity is null)
            {
                throw new NotFoundException(nameof(Reservation), user.Id);
            }

            entity.Status = ReservationStatus.CheckIn;

            var reservationTable = await _unitOfWork.ReservationTableRepository.GetAllAsync();
            reservationTable.RemoveAll(rt => !(rt.ReservationId == entity.Id));
            foreach (ReservationTable rt in reservationTable)
            {
                var table = await _unitOfWork.TableRepository.GetAsync(t => t.Id == rt.TableId);
                if (table is null)
                {
                    throw new NotFoundException(nameof(Reservation), rt.TableId);
                }
                table.Status = TableStatus.Occupied;
                await _unitOfWork.TableRepository.UpdateAsync(table);
            }

            var result = await _unitOfWork.ReservationRepository.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<ReservationDto>("error");
            }
            var mappedResult = _mapper.Map<ReservationDto>(result);

            return new Response<ReservationDto>(mappedResult);
        }
    }
}
