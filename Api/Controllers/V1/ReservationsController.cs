using Application.Reservations.Commands;
using Application.Reservations.Queries;
using Application.Reservations.Response;
using Application.Models;
using Core.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Application.Tables.Commands;
using Application.Tables.Response;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ReservationsController : ApiControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery] GetReservationWithPaginationQuery query)
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
                var response = new Response<PaginatedList<ReservationDto>>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }

                var query = new GetReservationWithIdQuery()
                {
                    Id = id
                };

                var result = await Mediator.Send(query);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var response = new Response<ReservationDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        [HttpGet("BusyDate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBusyTimeWithSetOfTable([FromQuery] GetBusyTimeOfDateQuery query)
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
                var response = new Response<ReservationDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostAsync([FromBody] CreateReservationCommand command)
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
                var response = new Response<ReservationDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutAsync(int id, [FromForm] ChangeReservationStatusCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                if (id != command.Id)
                {
                    var response = new Response<TableDto>("The Id do not match")
                    {
                        StatusCode = HttpStatusCode.BadRequest
                    };
                    return StatusCode((int)response.StatusCode, response);
                }

                var result = await Mediator.Send(command);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var response = new Response<TableDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        // DELETE api/Blogs/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }

                var result = await Mediator.Send(new DeleteReservationCommand { Id = id });
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var response = new Response<TableDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }
    }
}
