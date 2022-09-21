using Application.Categories.Response;
using Application.Menus.Commands;
using Application.Menus.Queries;
using Application.Menus.Response;
using Application.Models;
using Core.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public sealed class MenusController : ApiControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery] GetMenuWithPaginationQuery query)
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
                var response = new Response<PaginatedList<MenuDto>>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        [HttpGet("Available")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAvailableAsync([FromQuery] GetAvailableMenuQuery query)
        {
            try
            {
                var result = await Mediator.Send(query);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var response = new Response<PaginatedList<MenuDto>>(ex.Message)
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

                var query = new GetMenuWithIdQuery()
                {
                    Id = id
                };

                var result = await Mediator.Send(query);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var response = new Response<MenuDto>(ex.Message)
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
        public async Task<IActionResult> PostAsync([FromBody] CreateMenuCommand command)
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
                var response = new Response<MenuDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutAsync(int id, [FromBody] UpdateMenuCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                if (id != command.Id)
                {
                    var response = new Response<CategoryDto>("The Id do not match")
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
            catch (Exception ex)
            {
                var response = new Response<MenuDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var command = new DeleteMenuCommand
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
                var response = new Response<MenuDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }
    }
}
