﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.Services.TelegramServices.BaseMethods.HandleUpdate;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace Application.Services.TelegramServices.Interfaces
{
    public interface IHandleMessage
    {
        Task HandleMessageAsync(ITelegramBotClient client, Message message, UserSession userSession);
    }
}
