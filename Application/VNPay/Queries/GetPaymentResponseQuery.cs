using Application.Common.Exceptions;
using Application.Models;
using Application.Types.Response;
using Application.VNPay.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Application.VNPay.Queries
{
    public sealed class GetPaymentResponseQuery : IRequest<Response<PaymentDto>>
    {
        public string vnp_TxnRef { get; init; }
        public string vnp_ResponseCode { get; init; }
        public string vnp_TransactionStatus { get; init; }
        public string vnp_SecureHash { get; init; }
    }

    public sealed class GetPaymentResponseQueryHandler : IRequestHandler<GetPaymentResponseQuery, Response<PaymentDto>>
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetPaymentResponseQueryHandler(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _config = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaymentDto>> Handle(GetPaymentResponseQuery request, CancellationToken cancellationToken)
        {
            string paymentId = request.vnp_TxnRef;
            string responseCode = request.vnp_ResponseCode;
            string transactionStatus = request.vnp_TransactionStatus;
            string vnp_HashSecret = _config.GetSection("vnpay")["vnp_HashSecret"]; //Secret key
            string vnp_SecureHash = request.vnp_SecureHash;

            VnPayLibrary vnpay = new VnPayLibrary();
            //bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
            //if (checkSignature)
            //{
                if (responseCode == "00" && transactionStatus == "00")
                {
                    var entity = await _unitOfWork.PaymentRepository.GetAsync(e => e.Id == paymentId);
                    if (entity is null)
                    {
                        throw new NotFoundException(nameof(Order), paymentId);
                    }
                    entity.Status = PaymentStatus.Paid;

                    var entityOrder = await _unitOfWork.OrderRepository.GetAsync(e => e.Id == entity.OrderId);
                    if (entityOrder is null)
                    {
                        throw new NotFoundException(nameof(Order), paymentId);
                    }
                    entityOrder.Status = OrderStatus.Paid;

                    var result = await _unitOfWork.PaymentRepository.UpdateAsync(entity);
                    await _unitOfWork.OrderRepository.UpdateAsync(entityOrder);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    if (result is null)
                    {
                        return new Response<PaymentDto>("error");
                    }
                    var mappedResult = _mapper.Map<PaymentDto>(result);
                    return new Response<PaymentDto>()
                    {
                        Succeeded = true,
                        StatusCode = System.Net.HttpStatusCode.NoContent
                    };
                }
            //}
            return new Response<PaymentDto>()
            {
                Succeeded = false,
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
        }
    }
}
