﻿using Application.Common.Security;
using Application.Models;
using Application.Types.Response;
using Application.VNPay.Commands;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class BillingsController : ApiControllerBase
    {
        /// <summary>
        /// Create a Payment for Reservation.
        /// </summary>
        /// <returns>Billing for the reservation.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Billings/Reservation
        ///     {
        ///        "amount": 200000,
        ///        "reservationId": "2015"
        ///     }
        ///     
        /// </remarks>
        [HttpPost("Reservation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<BillingDto>>> CreateBillingForReservation([FromBody] CreateBillingForReservationCommand command)
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
                var response = new Response<BillingDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }
    }
}
