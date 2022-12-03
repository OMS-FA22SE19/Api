using Application.Common.Exceptions;
using Application.Helpers;
using Application.Models;
using Application.Reservations.Response;
using Application.Types.Response;
using Application.VNPay.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Application.VNPay.Commands
{
    public sealed class CreateBillingForReservationCommand : IRequest<Response<BillingDto>>
    {
        [Required]
        public int Amount { get; init; }
        [Required]
        public int ReservationId { get; init; }
    }

    public sealed class CreateBillingForReservationCommandHandler : IRequestHandler<CreateBillingForReservationCommand, Response<BillingDto>>
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CreateBillingForReservationCommandHandler(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _config = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<BillingDto>> Handle(CreateBillingForReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _unitOfWork.ReservationRepository.GetAsync(r => r.Id == request.ReservationId);
            if (reservation is null)
            {
                return new Response<BillingDto>($"Reservation {request.ReservationId} cannot be found!");
            }
            if (reservation.Status == ReservationStatus.Done || reservation.Status == ReservationStatus.CheckIn)
            {
                return new Response<BillingDto>($"This Reservation is already checked in or done");
            }

            string id = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
            var billing = await _unitOfWork.BillingRepository.GetAsync(b => b.ReservationId == request.ReservationId);
            var result = new Billing();
            if (billing is null)
            {
                Billing bill = new Billing
                {
                    Id = id,
                    ReservationId = request.ReservationId,
                    ReservationAmount = request.Amount
                };
                result = await _unitOfWork.BillingRepository.InsertAsync(bill);
            }
            else
            {
                billing.ReservationAmount = request.Amount;
                result = await _unitOfWork.BillingRepository.UpdateAsync(billing);
            }

            var order = await _unitOfWork.OrderRepository.GetAsync(o => o.ReservationId == request.ReservationId);
            if (order is not null)
            {
                order.PrePaid = request.Amount;
                await _unitOfWork.OrderRepository.UpdateAsync(order);
            }

            reservation.Status = ReservationStatus.Reserved;
            await _unitOfWork.ReservationRepository.UpdateAsync(reservation);

            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<BillingDto>("error");
            }

            var dto = new BillingDto()
            {
                Id = id,
                ReservationId = request.ReservationId,
                Amount = request.Amount
            };
            if (billing is not null)
            {
                dto.Id = billing.Id;
            }

            return new Response<BillingDto>(dto)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };

        }
    }
}
