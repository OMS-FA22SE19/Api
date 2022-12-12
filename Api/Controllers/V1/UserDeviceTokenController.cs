using Application.Common.Exceptions;
using Application.Common.Security;
using Application.Models;
using Application.UserDeviceTokens.Commands;
using Application.UserDeviceTokens.Queries;
using Application.UserDeviceTokens.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class UserDeviceTokensController : ApiControllerBase
    {
        /// <summary>
        /// Retrieve a All User Device Token.
        /// </summary>
        /// <returns>A List of User Device Token.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /UserDeviceTokens
        ///
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<UserDeviceTokenDto>>> GetAllAsync()
        {
            try
            {
                var query = new GetAllUserDeviceTokenQuery();

                var result = await Mediator.Send(query);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var response = new Response<UserDeviceTokenDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Retrieve a specific UserDeviceToken by Id.
        /// </summary>
        /// <returns>A UserDeviceToken.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /UserDeviceTokens/1
        ///
        /// </remarks>
        /// <param name="id">The desired id of UserDeviceToken</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<UserDeviceTokenDto>>> GetByIdAsync(string id)
        {
            try
            {
                if (id is null)
                {
                    return BadRequest();
                }

                var query = new GetUserDeviceTokenWithIdQuery()
                {
                    userId = id
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
                var response = new Response<UserDeviceTokenDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Create a User Device Token.
        /// </summary>
        /// <returns>New UserDeviceToken.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /UserDeviceTokens
        ///     {
        ///        "userId": "userId",
        ///        "deviceToken": "deviceToken"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<UserDeviceTokenDto>>> PostAsync([FromBody] AddUserDeviceTokenCommand command)
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
                var response = new Response<UserDeviceTokenDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }
    }
}
