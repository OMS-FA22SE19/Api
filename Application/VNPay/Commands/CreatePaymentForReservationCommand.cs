using Application.Common.Exceptions;
using Application.Helpers;
using Application.Models;
using Application.VNPay.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Application.VNPay.Commands
{
    public sealed class CreatePaymentForReservationCommand : IRequest<Response<PaymentUrlDto>>
    {
        [Required]
        public int Amount { get; init; }
        [Required]
        public int ReservationId { get; init; }
    }

    public sealed class CreatePaymentForReservationCommandHandler : IRequestHandler<CreatePaymentForReservationCommand, Response<PaymentUrlDto>>
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CreatePaymentForReservationCommandHandler(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _config = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaymentUrlDto>> Handle(CreatePaymentForReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _unitOfWork.ReservationRepository.GetAsync(r => r.Id == request.ReservationId);
            if (reservation is null)
            {
                return new Response<PaymentUrlDto>($"Reservation {request.ReservationId} cannot be found!");
            }

            string id = DateTime.Now.ToString("yyyyMMddHHmmss");
            var billing = await _unitOfWork.BillingRepository.GetAsync(b => b.ReservationId == request.ReservationId);
            var result = new Billing();
            if (billing is null)
            {
                Billing bill = new Billing
                {
                    Id = id,
                    ReservationId = request.ReservationId,
                    ReservationEBillingId = id
                };
                result = await _unitOfWork.BillingRepository.InsertAsync(bill);
            }
            else
            {
                if (billing.ReservationAmount == 0)
                {
                    billing.ReservationEBillingId = id;
                    result = await _unitOfWork.BillingRepository.UpdateAsync(billing);
                }
                else
                {
                    return new Response<PaymentUrlDto>($"Reservation {request.ReservationId} has been payed");
                }
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<PaymentUrlDto>("error");
            }


            //Get Config Info
            string vnp_Returnurl = _config.GetSection("vnpay")["vnp_Returnurl"] + "/Reservation/Response"; //URL nhan ket qua tra ve 
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
            //if (order is null)
            //{
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan cho reservation: " + request.ReservationId.ToString());
            //}
            //else
            //{
            //    vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang: " + order.Id.ToString());
            //}
            //vnpay.AddRequestData("vnp_OrderType", orderCategory.SelectedItem.Value); //default value: other
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", id.ToString());//order.OrderId.ToString());
            //Add Params of 2.1.0 Version
            vnpay.AddRequestData("vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"));

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            Console.WriteLine("VNPAY URL: {0}", paymentUrl);
            PaymentUrlDto paymentUrlDto = new PaymentUrlDto() { Url = paymentUrl };
            return new Response<PaymentUrlDto>(paymentUrlDto);
        }
    }
}
