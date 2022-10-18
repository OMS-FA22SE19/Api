using Application.Common.Exceptions;
using Application.Helpers;
using Application.Models;
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
    public sealed class CreatePaymentCommand : IRequest<Response<PaymentUrlDto>>
    {
        [Required]
        public int Amount { get; init; }
        [Required]
        public string OrderId { get; init; }
    }

    public sealed class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Response<PaymentUrlDto>>
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CreatePaymentCommandHandler(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _config = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaymentUrlDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.OrderRepository.GetAsync(e => e.Id == request.OrderId, $"{nameof(Order.OrderDetails)},{nameof(Order.User)}");
            if (entity is null)
            {
                throw new NotFoundException(nameof(Order), request.OrderId);
            }

            if (entity.OrderDetails.Any(e => e.Status != OrderDetailStatus.Served))
            {
                return new Response<PaymentUrlDto>($"Order {entity.Id} cannot be confirmed. Make sure all dishes have been served!");
            }

            if (entity.Status == OrderStatus.Paid)
            {
                return new Response<PaymentUrlDto>($"Order {entity.Id} cannot be Paid again!");
            }

            Payment payment = new Payment
            {
                Id = DateTime.Now.ToString("yyyyMMddHHmmss"),
                OrderId = request.OrderId.ToString(),
                Status = PaymentStatus.Processing,
                Amount = request.Amount
            };
            var result = await _unitOfWork.PaymentRepository.InsertAsync(payment);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<PaymentUrlDto>("error");
            }

            //Get Config Info
            string vnp_Returnurl = _config.GetSection("vnpay")["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = _config.GetSection("vnpay")["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = _config.GetSection("vnpay")["vnp_TmnCode"]; //Ma website
            string vnp_HashSecret = _config.GetSection("vnpay")["vnp_HashSecret"]; //Chuoi bi mat
            if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
            {
                throw new NotFoundException("Vui lòng cấu hình các tham số: vnp_TmnCode,vnp_HashSecret");

            }

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (request.Amount * 100).ToString());

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));//yyyyMMddHHmmss
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "http://localhost:5246");


            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang: " + request.OrderId.ToString());
            //vnpay.AddRequestData("vnp_OrderType", orderCategory.SelectedItem.Value); //default value: other
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", payment.Id.ToString());//order.OrderId.ToString());
            //Add Params of 2.1.0 Version
            vnpay.AddRequestData("vnp_ExpireDate", DateTime.Now.AddMinutes(5).ToString("yyyyMMddHHmmss"));

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            Console.WriteLine("VNPAY URL: {0}", paymentUrl);
            PaymentUrlDto paymentUrlDto = new PaymentUrlDto() { Url = paymentUrl };
            return new Response<PaymentUrlDto>(paymentUrlDto);
        }
    }
}
