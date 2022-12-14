using Application.Common.Exceptions;
using Application.Common.Security;
using Application.Models;
using Application.Orders.Commands;
using Application.Orders.Queries;
using Application.Orders.Response;
using Application.Reservations.Response;
using Core.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
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
        /// <returns>An Order.</returns>
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

        /// Retrieve current order of a table.
        /// </summary>
        /// <returns>An Order.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Orders/Table/1
        ///     
        /// </remarks>
        [HttpGet("Table/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<OrderDto>>> GetTableOrderAsync(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }

                var query = new GetTableCurrentOrderQuery()
                {
                    TableId = id
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

        /// Retrieve current order of a table.
        /// </summary>
        /// <returns>An Order.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Orders/Table/1
        ///     
        /// </remarks>
        [HttpGet("Reservation/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<OrderDto>>> GetReservationTable(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }

                var query = new GetOrderWithReservationQuery()
                {
                    reservationId = id
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
        ///        "reservationId": "9",
        ///        "orderDetails": {
        ///             "2": {
        ///             quantity: 2,
        ///             note: null
        ///             }
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

        /// <summary>
        /// Create an Prior order for reservation.
        /// </summary>
        /// <returns>New Prior order for reservation.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Orders/PriorFood
        ///     {
        ///        "reservationId": "9",
        ///        "orderDetails": {
        ///             "2": {
        ///             quantity: 2,
        ///             note: null
        ///             }
        ///         }
        ///     }
        ///     
        /// </remarks>
        [HttpPost("PriorFood")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreatePriorFoodOrder([FromBody] CreatePreOrderFoodForReservationCommand command)
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
        /// Update an Prior order for reservation.
        /// </summary>
        /// <returns>update Prior order for reservation.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /Orders/PriorFood/9
        ///     {
        ///        "reservationId": "9",
        ///        "orderDetails": {
        ///             "2": {
        ///             quantity: 2,
        ///             note: null
        ///             }
        ///         }
        ///     }
        ///     
        /// </remarks>
        [HttpPut("PriorFood/{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdatePriorFoodOrder(int id, [FromBody] UpdatePreOrderFoodForReservationCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                if (id != command.ReservationId)
                {
                    var response = new Response<ReservationDto>("The Id do not match")
                    {
                        StatusCode = HttpStatusCode.BadRequest
                    };
                    return StatusCode((int)response.StatusCode, response);
                }
                var result = await Mediator.Send(command);
                if (result.StatusCode == HttpStatusCode.NoContent)
                {
                    return NoContent();
                }
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
        ///     PUT /Orders/AddDishes
        ///     {
        ///        "orderId": "9",
        ///        "orderDetails": {
        ///             "2": {
        ///             quantity: 2,
        ///             note: null
        ///             }
        ///         }
        ///     }
        ///  
        ///     
        /// </remarks>
        [HttpPut("AddDishes")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddDishes([FromBody] AddNewDishesToOrderCommand command)
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

        /// Check a processing Order.
        /// 
        /// </summary>
        /// <returns>An Orders.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Orders/9-0939758999-07-10-2022-10:55:10/Check
        ///     
        /// </remarks>
        [HttpPut("{id}/Check")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CheckAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest();
                }
                var command = new CheckingBillCommand()
                {
                    Id = id
                };
                var result = await Mediator.Send(command);
                if (result.StatusCode == HttpStatusCode.NoContent)
                {
                    return NoContent();
                }
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
