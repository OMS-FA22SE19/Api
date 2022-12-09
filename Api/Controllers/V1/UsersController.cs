using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Security;
using Application.Models;
using Application.Users.Commands;
using Application.Users.Queries;
using Application.Users.Response;
using Core.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class UsersController : ApiControllerBase
    {
        private readonly ISendMailService _sendMailService;
        public UsersController(ISendMailService sendMailService) {
            _sendMailService = sendMailService;
        }

        /// <summary>
        /// Retrieve a list of User.
        /// </summary>
        /// <returns>List of Food Users.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Users
        ///     
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<PaginatedList<UserDto>>>> GetAsync([FromQuery] GetUserWithPaginationQuery query)
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
                var response = new Response<PaginatedList<UserDto>>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Confirm a user email.
        /// </summary>
        /// <returns>Confirm User.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Users/Confirm
        ///     {
        ///        "userId": "123",
        ///        "code": "0931118342"
        ///     }
        ///     
        /// </remarks>
        [HttpGet("ConfirmEmail/confirm")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<Response<UserDto>>> ConfirmEmail([FromQuery] ConfirmEmailCommand command)
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
                var response = new Response<UserDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Resent email confirm
        /// </summary>
        /// <returns>New User.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Users/ResentEmail
        ///     {
        ///        "userId": "123",
        ///        "code": "0931118342"
        ///     }
        ///     
        /// </remarks>
        [HttpPost("ResentEmail")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<Response<UserDto>>> ResentEmail([FromQuery] ResentEmailConfirmCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var result = await Mediator.Send(command);
                var url = Request.Scheme + "://" + Request.Host + Url.Action("ConfirmEmail", "Users", new {email = command.email, code = result.Message});
                var realUrl = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(url);
                realUrl = realUrl.Replace('-', '+');
                realUrl = realUrl.Replace('_', '/');
                var content = new MailContent()
                {
                    To = command.email,
                    Subject = "Access information to OMS",
                    Body = CreateBodyMessage(result.Message, realUrl)
                };
                await _sendMailService.SendMail(content);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var response = new Response<UserDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Retrieve a specific User by Id.
        /// </summary>
        /// <returns>A User.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Users/1
        ///
        /// </remarks>
        /// <param name="id">The desired id of Food User</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<UserDto>>> GetByIdAsync(string id)
        {
            try
            {
                if (id is null)
                {
                    return BadRequest();
                }

                var query = new GetUserWithIdQuery()
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
                var response = new Response<UserDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Register a User.
        /// </summary>
        /// <returns>New User.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Users
        ///     {
        ///        "FullName": "Quang",
        ///        "PhoneNumber": "0931118342",
        ///        "Email": "customerEmail@gmail.com",
        ///        "Password": "Password"
        ///     }
        ///     
        /// </remarks>
        [HttpPost("Register")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<Response<UserDto>>> PostAsync([FromBody] RegisterUserCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var result = await Mediator.Send(command);
                var url = Request.Scheme + "://" + Request.Host + Url.Action("ConfirmEmail", "Users", new { email = command.Email, code = result.Message });
                var realUrl = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(url);
                realUrl = realUrl.Replace('-', '+');
                realUrl = realUrl.Replace('_', '/');
                var content = new MailContent()
                {
                    To = command.Email,
                    Subject = "Access information to OMS",
                    Body = CreateBodyMessage(result.Message, realUrl)
                };
                await _sendMailService.SendMail(content);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var response = new Response<UserDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Create a User.
        /// </summary>
        /// <returns>New User.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Users
        ///     {
        ///        "FullName": "Quang",
        ///        "PhoneNumber": "0931118342",
        ///        "Email": "customerEmail@gmail.com",
        ///        "Role": "Customer",
        ///        "Password": "Password"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<UserDto>>> PostAsync([FromBody] CreateUserCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var result = await Mediator.Send(command);
                var url = Request.Scheme + "://" + Request.Host + Url.Action("ConfirmEmail", "Users", new { email = command.Email, code = result.Message });
                var realUrl = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(url);
                realUrl = realUrl.Replace('-', '+');
                realUrl = realUrl.Replace('_', '/');
                var content = new MailContent()
                {
                    To = command.Email,
                    Subject = "Access information to OMS",
                    Body = CreateBodyMessage(result.Message, realUrl)
                };
                await _sendMailService.SendMail(content);
                return StatusCode((int)result.StatusCode, result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var response = new Response<UserDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }
        

        /// <summary>
        /// Update a specific User.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /Users/1
        ///     {
        ///        "id": "1",
        ///        "fullName": "Le Van A",
        ///        "phoneNumber": "0931118342",
        ///        "role": "Customer"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">The id of updated User</param>
        [HttpPut("id")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutAsync(string Id, [FromBody] UpdateUserCommand command)
        {
            try
            {
                if (!ModelState.IsValid || Id is null)
                {
                    return BadRequest();
                }

                if (!Id.Equals(command.Id))
                {
                    var response = new Response<UserDto>("The Id do not match")
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
                var response = new Response<UserDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Delete a specific User.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /Users/1
        ///
        /// </remarks>
        /// <param name="id">The id of deleted User</param>
        [HttpDelete("id")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            try
            {
                var command = new DeleteUserCommand
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
                var response = new Response<UserDto>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        private string CreateBodyMessage(string code, string url)
        {
            return "<div lang=\"EN-US\" link=\"blue\" vlink=\"#954F72\">" +
                "<div class=\"m_-863817368153641209WordSection1\">" +
                "<p style=\"line-height:150%\"><span style=\"font-size:13.5pt;line-height:150%;color:black\">Quý khách vui lòng ấn vào đường link này để xác nhận:" + url + " </span><b></p>" +
                "</div>" +
                "</div>";
        }
    }
}
