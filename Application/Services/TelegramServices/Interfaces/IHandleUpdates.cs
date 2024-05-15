using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Application.Services.TelegramServices.Interfaces
{
    public interface IHandleUpdates
    {
        Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken = default);
    }
}
