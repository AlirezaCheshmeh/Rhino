using Application.Common;
using Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Domain.DTOs.Shared;
using Domain.Entities.Reminders;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Application.Extensions;
using Telegram.Bot.Types.ReplyMarkups;
using Application.Services.TelegramServices.ConstVariable;
using Telegram.Bot.Types.Enums;
using Application.Utility;

namespace Application.Mediator.Reminders.Command
{
    public class SendTelegramReminderCommand : ICommand<ServiceRespnse>
    {
        public class SendTelegramReminderCommandHandler : ICommandHandler<SendTelegramReminderCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<Reminder> _reminderRepository;
            private readonly IConfiguration _configuration;

            public SendTelegramReminderCommandHandler(IGenericRepository<Reminder> reminderRepository, IConfiguration configuration)
            {
                _reminderRepository = reminderRepository;
                _configuration = configuration;
            }
            public async Task<ServiceRespnse> Handle(SendTelegramReminderCommand request, CancellationToken cancellationToken)
            {
                var reminders = await _reminderRepository.GetAsNoTrackingQuery().Where(z => z.RemindDate.Date == DateTime.Now.Date && !z.IsExpire).ToListAsync();
                var _telegramBotClient = new TelegramBotClient(_configuration["TelegramSettings:TelegramKey"]);
                //telegram config
                foreach (var z in reminders)
                {
                   z.IsExpire = true;
                   List<InlineKeyboardButton> buttons = new();
                   buttons.Add(InlineKeyboardButton.WithCallbackData("👍 متوجه شدم", ConstCallBackData.Global.BackToMenu));
                   buttons.Add(InlineKeyboardButton.WithCallbackData("💭 یادآوری دوباره", ConstCallBackData.Reminder.RemindMeAgain + z.Id.ToString()));
                   var message = await _telegramBotClient.SendTextMessageAsync(z.ChatId, $"💰 با سلام جهت یادآوری به قیمت {z.Amount.ToString("N0").ToPersianNumber()} تومان \n 📔 بابت:{z.Description}", parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(buttons));
                   var res = await CacheExtension.UpdateValueAsync(z.TelegramId + ConstKey.ReminderMessageId, message.MessageId);
                   await Task.CompletedTask;
                };
                await _reminderRepository.UpdateRangeAsync(reminders, cancellationToken);
                return new ServiceRespnse().OK();

            }

            public async void TelegramSendMessage(string apilToken, string destID, string text)
            {
                string urlString = $"https://api.telegram.org/bot{apilToken}/sendMessage?chat_id={destID}&text={text}";

                WebClient webclient = new WebClient();

                webclient.DownloadStringAsync(new Uri(urlString));
            }
        }
    }
}
