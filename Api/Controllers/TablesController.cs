using Application.Tables.Commands.CreateTable;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TablesController : ApiControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(CreateTableCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
    }
}
