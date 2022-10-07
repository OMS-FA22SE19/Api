using Application.Common.Exceptions;
using Application.Models;
using Application.Orders.Commands;
using Application.Orders.Queries;
using Application.Orders.Response;
using Core.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class OrdersController : ApiControllerBase
    {
        /// <summary>
        /// Retrieve a list of Orders.
        /// </summary>
        /// <returns>List of Orders.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Orders
        ///     
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<PaginatedList<OrderDto>>>> GetAsync([FromQuery] GetOrderWithPaginationQuery query)
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
                var response = new Response<PaginatedList<OrderDto>>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// Retrieve a specific Order.
        /// </summary>
        /// <returns>An Orders.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Orders/9-0939758999-07-10-2022-10:55:10
        ///     
        /// </remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<OrderDto>>> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest();
                }

                var query = new GetOrderWithIdQuery()
                {
                    Id = id
                };

                var result = await Mediator.Send(query);
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
        /// Create an Order.
        /// </summary>
        /// <returns>New Order.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Orders
        ///     {
        ///        "tableId": "9",
        ///        "orderDetails": {
        ///             "2": 3 (FoodId: quantity),
        ///             "8": 2
        ///         }
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PostAsync([FromBody] CreateOrderCommand command)
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

        /// Confirm a processing Order.
        /// 
        /// </summary>
        /// <returns>An Orders.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Orders/9-0939758999-07-10-2022-10:55:10/Confirm
        ///     
        /// </remarks>
        [HttpPost("{id}/Confirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ConfirmAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest();
                }
                var command = new ConfirmPaymentOrderCommand()
                {
                    Id = id
                };
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
