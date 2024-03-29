﻿using Application.Common.Interfaces;
using Application.Helpers;
using Application.Models;
using Application.Reservations.Response;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Reservations.Queries
{
    public class GetBusyTimeOfDateQuery : IRequest<Response<List<BusyTimeDto>>>
    {
        [Required]
        public DateTime Date { get; init; }
        [Required]
        public int NumOfSeats { get; set; }
        [Required]
        public int TableTypeId { get; set; }
        [Required]
        public int Quantity { get; set; }
        public int ReservationId { get; set; }
    }

    public class GetBusyTimeOfDateQueryHandler : IRequestHandler<GetBusyTimeOfDateQuery, Response<List<BusyTimeDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetBusyTimeOfDateQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Response<List<BusyTimeDto>>> Handle(GetBusyTimeOfDateQuery request, CancellationToken cancellationToken)
        {
            var reservations = await _unitOfWork.ReservationRepository.GetAllReservationWithDate(request.Date, request.TableTypeId, request.NumOfSeats);
            if (reservations.Any() && request.ReservationId != 0)
            {
                var currentReservation = reservations.FirstOrDefault(r => r.Id == request.ReservationId);
                if (currentReservation != null)
                {
                    reservations.Remove(currentReservation);
                }
            }

            var tables = await _unitOfWork.TableRepository.GetTableOnNumOfSeatAndType(request.NumOfSeats, request.TableTypeId);
            bool isDefaultCustomer = (_currentUserService?.Role?.Equals("Customer") != true) || _currentUserService?.UserName?.Equals("DefaultCustomer") == true;
            var settings = await _unitOfWork.AdminSettingRepository.GetAllAsync();
            var busyTimes = DateTimeHelpers.GetBusyDateOfTable(request.Quantity, reservations, tables.Count, settings, isDefaultCustomer);

            return new Response<List<BusyTimeDto>>(busyTimes);
        }
    }
}
