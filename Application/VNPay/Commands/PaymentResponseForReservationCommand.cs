﻿using Application.Common.Exceptions;
using Application.Helpers;
using Application.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.VNPay.Commands
{
    public sealed class PaymentResponseForReservationCommand : IRequest<Response<PaymentDto>>
    {
        public string Vnp_TxnRef { get; init; }
        public string Vnp_ResponseCode { get; init; }
        public string Vnp_TransactionStatus { get; init; }
        public string Vnp_SecureHash { get; init; }
    }

    public sealed class CheckPaymentForReservationCommandHandler : IRequestHandler<PaymentResponseForReservationCommand, Response<PaymentDto>>
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CheckPaymentForReservationCommandHandler(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _config = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaymentDto>> Handle(PaymentResponseForReservationCommand request, CancellationToken cancellationToken)
        {
            string paymentId = request.Vnp_TxnRef;
            string responseCode = request.Vnp_ResponseCode;
            string transactionStatus = request.Vnp_TransactionStatus;
            string vnp_HashSecret = _config.GetSection("vnpay")["vnp_HashSecret"]; //Secret key
            string vnp_SecureHash = request.Vnp_SecureHash;

            VnPayLibrary vnpay = new VnPayLibrary();
            if (responseCode == "00" && transactionStatus == "00")
            {
                var entity = await _unitOfWork.PaymentRepository.GetAsync(e => e.Id == paymentId);
                if (entity is null)
                {
                    throw new NotFoundException(nameof(Order), paymentId);
                }
                entity.Status = PaymentStatus.Paid;

                var reservation = await _unitOfWork.ReservationRepository.GetAsync(r => r.Id == entity.ReservationId);
                reservation.Status = ReservationStatus.Reserved;
                await _unitOfWork.ReservationRepository.UpdateAsync(reservation);

                var result = await _unitOfWork.PaymentRepository.UpdateAsync(entity);
                await _unitOfWork.CompleteAsync(cancellationToken);
                if (result is null)
                {
                    return new Response<PaymentDto>("error");
                }
                var mappedResult = _mapper.Map<PaymentDto>(result);
                return new Response<PaymentDto>(mappedResult);
            }
            else
            {
                var result = await _unitOfWork.PaymentRepository.DeleteAsync(p => p.Id == paymentId);
                await _unitOfWork.CompleteAsync(cancellationToken);
                if (!result)
                {
                    return new Response<PaymentDto>("error");
                }
                return new Response<PaymentDto>("Transaction failed")
                {
                    Succeeded = false
                };
            }
        }
    }
}
