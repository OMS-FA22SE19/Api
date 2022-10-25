using Application.Models;
using Application.Orders.Commands;
using Application.Orders.Response;
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
        /// Create a Payment.
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
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<PaymentUrlDto>>> GetPaymentUrl([FromBody] CreatePaymentCommand query)
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
        [HttpGet("Check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<OrderDto>>> GetPaymentResponse([FromQuery] CheckPaymentCommand query)
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
                    if (result.Data.OrderId is not null)
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
