using Application.Common.Exceptions;
using Application.Foods.Commands;
using Application.Foods.Queries;
using Application.Foods.Response;
using Application.Common.Models;
using Application.Types.Response;
using Core.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public sealed class FoodsController : ApiControllerBase
    {
        /// <summary>
        /// Retrieve a list of Foods.
        /// </summary>
        /// <returns>List of Foods.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Foods
        ///
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<PaginatedList<FoodDto>>>> GetAsync([FromQuery] GetFoodWithPaginationQuery query)
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
                var response = new Response<PaginatedList<FoodDto>>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Retrieve a specific Food by Id.
        /// </summary>
        /// <returns>A Food.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Foods/1
        ///
        /// </remarks>
        /// <param name="id">The desired id of Food</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<FoodDto>>> GetByIdAsync(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }

                var query = new GetFoodWithIdQuery()
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
                var response = new Response<FoodDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Create a Food.
        /// </summary>
        /// <returns>New Food.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Foods
        ///     {
        ///        "name": "Pho",
        ///        "description": "Vietnamese food speciality",
        ///        "ingredient": "Noodle, beef, soup,
        ///        "available": true,
        ///        "picture" : (upload picture),
        ///        "courseTypeId": 1,
        ///        "types" : [1, 2, 5] (array of FoodType ids)
        ///     }
        ///
        /// </remarks>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<FoodDto>>> PostAsync([FromForm] CreateFoodCommand command)
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
                var response = new Response<FoodDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Update a specific Food.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /Foods/1
        ///     {
        ///        "id": 1,
        ///        "name": "Pho",
        ///        "description": "Vietnamese food speciality",
        ///        "ingredient": "Noodle, beef, soup",
        ///        "available": true,
        ///        "picture" : (upload picture),
        ///        "courseTypeId": 1,
        ///        "types" : [1, 2, 5] (array of FoodType ids)
        ///     }
        ///
        /// </remarks>
        /// <param name="id">The id of updated Food</param>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutAsync(int id, [FromForm] UpdateFoodCommand command)
        {
            try
            {
                if (!ModelState.IsValid || id < 0)
                {
                    return BadRequest();
                }
                if (id != command.Id)
                {
                    var response = new Response<TypeDto>("The Id do not match")
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
                var response = new Response<FoodDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Delete a specific Food.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /Foods/1
        ///
        /// </remarks>
        /// <param name="id">The id of deleted Food</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }

                var command = new DeleteFoodCommand
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
            catch (Exception ex)
            {
                var response = new Response<FoodDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Recover a deleted Food.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /Foods/1/Recover
        ///
        /// </remarks>
        /// <param name="id">The id of deleted Food</param>
        [HttpPut("{id}/Recover")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RestoreAsync(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }

                var command = new RecoverFoodCommand
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
            catch (Exception ex)
            {
                var response = new Response<FoodDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }
    }
}

