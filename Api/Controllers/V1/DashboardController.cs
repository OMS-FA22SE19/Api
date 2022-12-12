using Application.Common.Security;
using Application.Dashboard.Queries;
using Application.Dashboard.Response;
using Application.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class DashboardController : ApiControllerBase
    {
        /// <summary>
        /// Retrieve a list of royal customers.
        /// </summary>
        /// <returns>List of Royal Customers.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Dashboard/Customers
        ///     
        /// </remarks>
        [HttpGet("Customers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<CustomerStatistic>>> GetCustomerStatisticAsync([FromQuery] GetCustomerStatisticQuery query)
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
                var response = new Response<CustomerStatistic>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Retrieve a list of food quantity.
        /// </summary>
        /// <returns>List of Food Quantity.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Dashboard/Food
        ///     
        /// </remarks>
        [HttpGet("Food")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<FoodStatistic>>> GetFoodStatisticAsync([FromQuery] GetFoodStatisticQuery query)
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
                var response = new Response<FoodStatistic>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Retrieve a list of Trending Food.
        /// </summary>
        /// <returns>List of Trending Food.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Dashboard/Food
        ///     
        /// </remarks>
        [HttpGet("TrendingFood")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<List<MonthlyTrendingFood>>>> GetMontlyTrendingFood([FromQuery] GetMonthlyTrendingFoodQuery query)
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
                var response = new Response<List<MonthlyTrendingFood>>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Retrieve a list of monthly orders and reservations.
        /// </summary>
        /// <returns>List  of monthly Orders and Reservations.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Dashboard/OrdersReservations
        ///     
        /// </remarks>
        [HttpGet("OrdersReservations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<FoodStatistic>>> GetMonthlyOrdersReservationsAsync([FromQuery] GetMonthlyOrderReservationQuery query)
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
                var response = new Response<ActiveOrdersReservations>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }

        /// <summary>
        /// Retrieve a list of Royal Customers.
        /// </summary>
        /// <returns>List of Royal Customers.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Dashboard/Food
        ///     
        /// </remarks>
        [HttpGet("RoyalCustomers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<List<MonthlyTrendingFood>>>> GetRoyalCustomersAsync([FromQuery] GetRoyalCustomersQuery query)
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
                var response = new Response<List<RoyalCustomer>>(ex.Message)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                return StatusCode((int)response.StatusCode, response);
            }
        }
    }
}
