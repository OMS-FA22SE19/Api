using Application.Common.Exceptions;
using Application.Foods.Queries;
using Application.Foods.Response;
using Application.MenuFoods.Commands;
using Application.Menus.Commands;
using Application.Menus.Queries;
using Application.Menus.Response;
using Application.Models;
using Application.Types.Response;
using Core.Common;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public sealed class MenusController : ApiControllerBase
    {
        /// <summary>
        /// Retrieve a list of Menus.
        /// </summary>
        /// <returns>List of Menus.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Menus
        ///     
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<PaginatedList<MenuDto>>>> GetAsync([FromQuery] GetMenuWithPaginationQuery query)
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

        /// <summary>
        /// Retrieve available Menu.
        /// </summary>
        /// <returns>A Menu.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Menus/Available
        ///
        /// </remarks>
        [HttpGet("Available")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<MenuDto>>> GetAvailableAsync([FromQuery] GetAvailableMenuQuery query)
        {
            try
            {
                var result = await Mediator.Send(query);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (NotFoundException)
            {
                throw;
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

        /// <summary>
        /// Retrieve a specific Menu by Id.
        /// </summary>
        /// <returns>A Menu.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Menus/1
        ///
        /// </remarks>
        /// <param name="id">The desired id of Menu</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<MenuDto>>> GetByIdAsync(int id)
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
            catch (NotFoundException)
            {
                throw;
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

        /// <summary>
        /// Create a Menu.
        /// </summary>
        /// <returns>New Menu.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Menus
        ///     {
        ///        "name": "Casual",
        ///        "description": "An ordinary menu"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<MenuDto>>> PostAsync([FromBody] CreateMenuCommand command)
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
            catch (ValidationException)
            {
                throw;
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

        /// <summary>
        /// Update a Menu.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /Menus/1
        ///     {
        ///        "id": 1,
        ///        "name": "Casual",
        ///        "description": "An ordinary menu",
        ///        "available": false
        ///     }
        ///     
        /// </remarks>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutAsync(int id, [FromBody] UpdateMenuCommand command)
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
            catch (ValidationException)
            {
                throw;
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

        /// <summary>
        /// Delete a Menu.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /Menus/1
        ///     
        /// </remarks>
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

        /// <summary>
        /// Add an existing food to menu with price
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Menus/1/Food/4
        ///     {
        ///        "price": 100000
        ///     }
        ///     
        /// </remarks>
        /// <param name="id">The desired id of Menu</param>
        /// <param name="foodId">The desired id of Food</param>
        /// <param name="price">The price of Food in selected Menu</param>
        [HttpPost("{id}/Food/{foodId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddFoodToMenuAsync(int id, int foodId, [FromBody] double price)
        {
            try
            {
                if (id < 0 || foodId < 0)
                {
                    return BadRequest();
                }
                if (price < 0)
                {
                    return BadRequest($"{nameof(MenuFood.Price)} must have positive value");
                }
                var command = new AddExistingFoodToMenuCommand()
                {
                    Id = id,
                    FoodId = foodId,
                    Price = price
                };
                var result = await Mediator.Send(command);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ValidationException)
            {
                throw;
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

        /// <summary>
        /// Update food to menu with price
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /Menus/1/Food/4
        ///     {
        ///        "price": 100000
        ///     }
        ///     
        /// </remarks>
        /// <param name="id">The desired id of Menu</param>
        /// <param name="foodId">The desired id of Food</param>
        /// <param name="price">The price of Food in selected Menu</param>
        [HttpPut("{id}/Food/{foodId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateFoodInMenuAsync(int id, int foodId, [FromBody] UpdateFoodOfMenuCommand command)
        {
            try
            {
                if (id < 0 || foodId < 0)
                {
                    return BadRequest();
                }
                command.Id = id;
                command.FoodId = foodId;
                var result = await Mediator.Send(command);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ValidationException)
            {
                throw;
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

        /// <summary>
        /// Add new food to menu with price
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Menus/1/NewFood
        ///     {
        ///        "name": "Pho",
        ///        "description": "Vietnamese food speciality",
        ///        "ingredient": "Noodle, beef, soup",
        ///        "available": true,
        ///        "picture" : (upload picture),
        ///        "courseTypeId": 1,
        ///        "types" : [1, 2, 5] (array of FoodType ids),
        ///        "price": 100000
        ///     }
        ///     
        /// </remarks>
        /// <param name="id">The desired id of Menu</param>
        [HttpPost("{id}/NewFood")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddFoodToMenuAsync(int id, [FromForm] AddNewFoodToMenuCommand command)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                command.Id = id;
                var result = await Mediator.Send(command);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ValidationException)
            {
                throw;
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

        /// <summary>
        /// Retrieve a list of Food in Menu with Id.
        /// </summary>
        /// <returns>List of Foods.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Menus/Food
        ///     
        /// </remarks>
        /// <param name="menuId">The desired id of Menu</param>
        /// <param name="courseTypeId">The MenuId of the Foods</param>
        /// <param name="typeId">The TypeId of the Foods</param>
        [HttpGet("{menuId}/Food")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<List<MenuDto>>>> GetFoodWithMenuIdAsync(int menuId, [FromQuery] GetFoodWithMenuIdQuery query)
        {
            try
            {
                if (menuId < 0)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                query.MenuId = menuId;
                var result = await Mediator.Send(query);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var response = new Response<List<MenuFoodDto>>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Retrieve a list of Food in Menu with Id.
        /// </summary>
        /// <returns>List of Foods.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Menus/Food
        ///     
        /// </remarks>
        /// <param name="menuId">The desired id of Menu</param>
        /// <param name="foodId">The desired id of Food</param>
        [HttpDelete("{menuId}/Food/{foodId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<List<MenuDto>>>> RemoveFoodFromMenuAsync(int menuId, int foodId)
        {
            try
            {
                if (menuId < 0 || foodId < 0)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                var query = new RemoveFoodFromMenuCommand()
                {
                    MenuId = menuId,
                    FoodId = foodId
                };
                var result = await Mediator.Send(query);
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
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var response = new Response<List<MenuFoodDto>>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Recover a deleted Menu.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /Menus/1/Recover
        ///
        /// </remarks>
        /// <param name="id">The id of deleted Menu</param>
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

                var command = new RecoverMenuCommand
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
