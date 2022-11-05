﻿using Application.Common.Exceptions;
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
    }
}