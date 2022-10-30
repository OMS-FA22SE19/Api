using Application.Models;
using Application.Orders.Commands;
using Application.Orders.Response;
using Application.Types.Response;
using Application.VNPay.Commands;
using Application.VNPay.Response;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public sealed class VNPayController : ApiControllerBase
    {
        /// <summary>
        /// Create a Payment for Reservation.
        /// </summary>
        /// <returns>Url redirect to VNPay.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /VNPay
        ///     {
        ///        "Amount": 200000,
        ///        "OrderId": "1-0939758999-14-10-2022-21:07:57"
        ///     }
        ///     
        /// </remarks>
        [HttpPost("Reservation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<PaymentUrlDto>>> GetPaymentUrlForPayment([FromBody] CreatePaymentForReservationCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var url = Request.Scheme + "://" + Request.Host.Value;
                var result = await Mediator.Send(command);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var response = new Response<PaymentUrlDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Check payment.
        /// </summary>
        /// <returns>Payment.</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("Reservation/Response")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<BillingDto>>> GetPaymentForReservationResponse([FromQuery] PaymentResponseForReservationCommand query)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var result = await Mediator.Send(query);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var response = new Response<OrderDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Create a Payment for Order.
        /// </summary>
        /// <returns>Url redirect to VNPay.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /VNPay/Order
        ///     {
        ///        "Amount": 200000,
        ///        "OrderId": "1-0939758999-14-10-2022-21:07:57"
        ///     }
        ///     
        /// </remarks>
        [HttpPost("Order")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<PaymentUrlDto>>> GetPaymentUrlForOrder([FromBody] CreatePaymentForOrderCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var result = await Mediator.Send(command);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var response = new Response<PaymentUrlDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Check payment.
        /// </summary>
        /// <returns>Payment.</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("Order/Response")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<OrderDto>>> GetPaymentForOrderResponse([FromQuery] PaymentResponseForOrderCommand query)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var result = await Mediator.Send(query);
                if (result.Succeeded)
                {
                    ConfirmPaymentOrderCommand confirmQuery = new ConfirmPaymentOrderCommand()
                    {
                        Id = result.Data.OrderId
                    };
                    var billResult = await Mediator.Send(confirmQuery);
                    return StatusCode((int)billResult.StatusCode, billResult);
                }
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var response = new Response<OrderDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }
    }
}
