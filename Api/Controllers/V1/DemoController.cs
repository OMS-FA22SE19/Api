using Application.AdminSettings.Response;
using Application.Common.Exceptions;
using Application.Common.Security;
using Application.Demo.Commands;
using Application.Models;
using Application.Orders.Commands;
using Application.Orders.Response;
using Application.Reservations.Commands;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class DemoController : ApiControllerBase
    {
        /// <summary>
        /// Add demo Reservation.
        /// </summary>
        /// <returns>List of id generated.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Demo/Reservation
        ///
        /// </remarks>
        [HttpPost("Reservation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<List<AdminSettingDto>>>> DemoCreateReservation([FromBody] CreateReservationDemo request)
        {
            try
            {
                var result = await Mediator.Send(request);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var response = new Response<List<AdminSettingDto>>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Create an Order Demo.
        /// </summary>
        /// <returns>New Order for Demo.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Demo/Order
        ///     {
        ///        "reservationId": [1, 2],
        ///        "orderDetails": {
        ///             "2": {
        ///             quantity: 2,
        ///             note: null
        ///             }
        ///         }
        ///     }
        ///     
        /// </remarks>
        [HttpPost("Order")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PostAsync([FromBody] AddOrderToReservationDemo command)
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
            catch (NotFoundException)
            {
                throw;
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
        /// Create an Order Demo.
        /// </summary>
        /// <returns>New Order for Demo.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Demo/OrderDetailStatus
        ///     {
        ///        "numOfProcessing": 10
        ///     }
        ///     
        /// </remarks>
        [HttpPost("OrderDetailStatus")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ChangeOrderDetailStatus([FromBody] ChangeOrderDetailStatusDemo command)
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
            catch (NotFoundException)
            {
                throw;
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
        /// Change Order status to checking.
        /// </summary>
        /// <returns>New Order for Demo.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Demo/OrderToChecking
        ///     {
        ///        "OrderIdsToChecking": ["string"]
        ///     }
        ///     
        /// </remarks>
        [HttpPost("OrderToChecking")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> OrderToChecking([FromBody] ChangeDemoOrderToChecking command)
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
            catch (NotFoundException)
            {
                throw;
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
        /// Change Order status to Done.
        /// </summary>
        /// <returns>New Order for Demo.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Demo/OrderToDone
        ///     {
        ///        "OrderIdsToChecking": ["string"]
        ///     }
        ///     
        /// </remarks>
        [HttpPost("OrderToDone")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> OrderToDone([FromBody] ChangeDemoOrderToDone command)
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
            catch (NotFoundException)
            {
                throw;
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
        /// Change Order status to pay by user.
        /// </summary>
        /// <returns>Order to pay for Demo.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Demo/PayOrderDemo
        ///     {
        ///        "OrderIdsToPay": [
        ///             "someid"
        ///        ]
        ///     }
        ///     
        /// </remarks>
        [HttpPost("PayOrderDemo")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PayOrderDemo([FromBody] PayOrderDemo command)
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
            catch (NotFoundException)
            {
                throw;
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
