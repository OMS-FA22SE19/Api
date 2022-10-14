using Application.Models;
using Application.Orders.Commands;
using Application.Orders.Response;
using Application.VNPay.Queries;
using Application.VNPay.Response;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public sealed class VNPayController : ApiControllerBase
    {
        [HttpGet("GetUrl")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaymentUrl([FromQuery] GetPaymentUrlQuery query)
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

        [HttpGet("Response")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaymentResponse([FromQuery] GetPaymentResponseQuery query)
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
                        Id = result.Data.orderId
                    };
                    var billResult = await Mediator.Send(confirmQuery);
                    return StatusCode((int)billResult.StatusCode, billResult);
                }
                else return StatusCode((int)result.StatusCode, result);
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
