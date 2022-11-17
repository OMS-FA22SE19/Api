using Application.Common.Exceptions;
using Application.Helpers;
using Application.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.VNPay.Commands
{
    public sealed class PaymentResponseForOrderCommand : IRequest<Response<BillingDto>>
    {
        public string Vnp_TxnRef { get; init; }
        public string Vnp_Amount { get; init; }
        public string Vnp_ResponseCode { get; init; }
        public string Vnp_TransactionStatus { get; init; }
        public string Vnp_SecureHash { get; init; }
    }

    public sealed class PaymentResponseForOrderCommandHandler : IRequestHandler<PaymentResponseForOrderCommand, Response<BillingDto>>
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public PaymentResponseForOrderCommandHandler(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _config = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<BillingDto>> Handle(PaymentResponseForOrderCommand request, CancellationToken cancellationToken)
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
                var entity = await _unitOfWork.BillingRepository.GetAsync(e => e.OrderEBillingId == paymentId);
                if (entity is null)
                {
                    throw new NotFoundException($"Can not find billing with Order EBilling Id: {paymentId}");
                }
                var order = await _unitOfWork.OrderRepository.GetAsync(o => o.Id.Equals(entity.OrderId));

                entity.OrderAmount = Convert.ToDouble(amount) / 100;

                var result = await _unitOfWork.BillingRepository.UpdateAsync(entity);
                await _unitOfWork.CompleteAsync(cancellationToken);
                if (result is null)
                {
                    return new Response<BillingDto>("error");
                }
                var mappedResult = _mapper.Map<BillingDto>(result);
                mappedResult.OrderId = order.Id;
                return new Response<BillingDto>(mappedResult);
            }
            else
            {
                return new Response<BillingDto>("Transaction failed")
                {
                    Succeeded = false
                };
            }
        }
    }
}
