using Application.AdminSettings.Commands;
using Application.AdminSettings.Queries;
using Application.AdminSettings.Response;
using Application.Common.Exceptions;
using Application.CourseTypes.Commands;
using Application.CourseTypes.Queries;
using Application.CourseTypes.Response;
using Application.Models;
using Core.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public sealed class AdminSettingsController : ApiControllerBase
    {
        /// <summary>
        /// Retrieve Admin Settings.
        /// </summary>
        /// <returns>Admin Settings.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /AdminSettings
        ///
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<List<AdminSettingDto>>>> GetAdminSettingAsync([FromQuery] GetAdminSettingQuery query)
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
                var response = new Response<List<AdminSettingDto>>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Update admin settings.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /AdminSettings
        ///     {
        ///        "openingTime": "10AM",
        ///        "endingTime": "11PM",
        ///        "maximumHourOfReservation": "1"
        ///     }
        ///
        /// </remarks>
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutAsync([FromBody] UpdateAdminSettingCommand command)
        {
            try
            {
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
                var response = new Response<CourseTypeDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }
    }
}
