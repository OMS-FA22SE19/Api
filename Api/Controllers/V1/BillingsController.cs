using Application.Models;
using Application.VNPay.Commands;
using Application.VNPay.Response;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public sealed class BillingsController : ApiControllerBase
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
        ///        "reservationId": "2015"
        ///     }
        ///     
        /// </remarks>
        [HttpPost("Reservation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<PaymentUrlDto>>> CreateBillingForReservation([FromBody] CreateBillingForReservationCommand command)
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
    }
}
