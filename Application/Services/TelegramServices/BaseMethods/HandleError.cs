using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot;
using Application.Services.TelegramServices.Configurations.Base;

namespace Application.Services.TelegramServices.BaseMethods
{
    public class HandleError : BaseConfig
    {

        public async Task HandleerrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken = default)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API has Error ===> ErrorCode:{apiRequestException.ErrorCode}  ErrorMessage:{apiRequestException.Message}"
            };

            Console.WriteLine(errorMessage);

        }
    }
}
