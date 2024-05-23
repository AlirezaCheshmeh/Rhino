﻿using Application.Cqrs.Queris;
using Application.Mediator.Banks.Query;
using Application.Mediator.Categories.Query;
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

        public TelegramController(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Test()
        {
            return Ok(await _queryDispatcher.SendAsync(new GetOutBoundTransactionTodaySummary { TelegramId = 5690372630 }));
        }
    }
}
