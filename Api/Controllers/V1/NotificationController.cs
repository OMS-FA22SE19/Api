using Application.Common.Exceptions;
using Application.Menus.Commands;
using Application.Menus.Response;
using Application.Models;
using Application.Notifications.Commands;
using Application.Notifications.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class NotificationController : ApiControllerBase
    {
        /// <summary>
        /// Create a Notification.
        /// </summary>
        /// <returns>New Notification.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Notification
        ///     {
        ///        "token": "Some_Token",
        ///        "title": "title",
        ///        "body": "This is a body"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<NotificationDto>>> PostAsync([FromBody] PushNotificationCommands command)
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
                var response = new Response<MenuDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }
        /// <summary>
        /// Create a Notification for multiple device.
        /// </summary>
        /// <returns>New Notification for multiple device.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Notification
        ///     {
        ///        "token": ["Some_Token", "Some_Token2"],
        ///        "title": "title",
        ///        "body": "This is a body"
        ///     }
        ///     
        /// </remarks>
        [HttpPost("MultipleDevice")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<NotificationDto>>> SendToMultipleDevice([FromBody] PushNotificationToMultipleDeviceCommand command)
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
                var response = new Response<MenuDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }
        /// <summary>
        /// Create a Notification for topic.
        /// </summary>
        /// <returns>New Notification for topic.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Notification
        ///     {
        ///        "topic": "Some_Topic",
        ///        "title": "title",
        ///        "body": "This is a body"
        ///     }
        ///     
        /// </remarks>
        [HttpPost("Topic")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<NotificationDto>>> SendToTopic([FromBody] PushNotificationToTopicCommand command)
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
                var response = new Response<MenuDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Create a Notification for topic.
        /// </summary>
        /// <returns>New Notification for topic.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Notification
        ///     {
        ///        "topic": "Some_Topic",
        ///        "title": "title",
        ///        "body": "This is a body"
        ///     }
        ///     
        /// </remarks>
        [HttpPost("Staff")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<NotificationDto>>> CallStaff([FromBody] CallStaffCommand command)
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
                var response = new Response<MenuDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }
    }

}
