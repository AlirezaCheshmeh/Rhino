using Application.Cqrs.Commands;
using Application.Cqrs.Queris;
using Application.Mediator.Banks.Query;
using Application.Mediator.Categories.Query;
using Application.Mediator.Reminders.Command;
using Application.Mediator.Transactions.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramController : ControllerBase
    {

        private readonly IQueryDispatcher   _queryDispatcher;
        private readonly ICommandDispatcher   _commandDispatcher;

        public TelegramController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Test()
        {
            return Ok(await _queryDispatcher.SendAsync(new GetAllBankQuery { Count = 10,PageNumber=1}));
        }
    }
}
