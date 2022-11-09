using Application.Common.Exceptions;
using Application.Helpers;
using Application.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.VNPay.Commands
{
    public sealed class PaymentResponseForReservationCommand : IRequest<Response<BillingDto>>
    {
        public string Vnp_TxnRef { get; init; }
        public string Vnp_Amount { get; init; }
        public string Vnp_ResponseCode { get; init; }
        public string Vnp_TransactionStatus { get; init; }
        public string Vnp_SecureHash { get; init; }
    }

    public sealed class PaymentResponseForReservationCommandHandler : IRequestHandler<PaymentResponseForReservationCommand, Response<BillingDto>>
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public PaymentResponseForReservationCommandHandler(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _config = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<BillingDto>> Handle(PaymentResponseForReservationCommand request, CancellationToken cancellationToken)
        {
            string paymentId = request.Vnp_TxnRef;
            string amount = request.Vnp_Amount;
            string responseCode = request.Vnp_ResponseCode;
            string transactionStatus = request.Vnp_TransactionStatus;
            string vnp_HashSecret = _config.GetSection("vnpay")["vnp_HashSecret"]; //Secret key
            string vnp_SecureHash = request.Vnp_SecureHash;

            VnPayLibrary vnpay = new VnPayLibrary();
            if (responseCode == "00" && transactionStatus == "00")
            {
                var entity = await _unitOfWork.BillingRepository.GetAsync(e => e.ReservationEBillingId == paymentId);
                if (entity is null)
                {
                    throw new NotFoundException($"Can not find billing with Reservation EBilling Id: {paymentId}");
                }

                entity.ReservationAmount = Convert.ToDouble(amount) / 100;

                var reservation = await _unitOfWork.ReservationRepository.GetAsync(r => r.Id == entity.ReservationId);
                reservation.Status = ReservationStatus.Reserved;
                await _unitOfWork.ReservationRepository.UpdateAsync(reservation);

                var result = await _unitOfWork.BillingRepository.UpdateAsync(entity);
                await _unitOfWork.CompleteAsync(cancellationToken);
                if (result is null)
                {
                    return new Response<BillingDto>("error");
                }
                var mappedResult = _mapper.Map<BillingDto>(result);
                mappedResult.ReservationId = reservation.Id;
                mappedResult.Amount = entity.ReservationAmount;
                return new Response<BillingDto>(mappedResult);
            }
            else
            {
                var result = await _unitOfWork.BillingRepository.DeleteAsync(p => p.Id == paymentId);
                await _unitOfWork.CompleteAsync(cancellationToken);
                if (!result)
                {
                    return new Response<BillingDto>("error");
                }
                return new Response<BillingDto>("Transaction failed")
                {
                    Succeeded = false
                };
            }
        }
    }
}
