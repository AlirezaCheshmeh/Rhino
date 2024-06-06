using Application.Common;
using Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Domain.DTOs.Shared;
using Domain.Entities.Reminders;
using Microsoft.Extensions.Configuration;
using Application.Extensions;
using Telegram.Bot.Types.Enums;

namespace Application.Mediator.Reminders.Command
{
    public class SendSnewsTelegramReminderCommand : ICommand<ServiceRespnse>
    {
        public class SendSnewsTelegramReminderCommandHandler : ICommandHandler<SendSnewsTelegramReminderCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<Reminder> _reminderRepository;
            private readonly IConfiguration _configuration;
            public SendSnewsTelegramReminderCommandHandler(IGenericRepository<Reminder> reminderRepository, IConfiguration configuration)
            {
                _reminderRepository = reminderRepository;
                _configuration = configuration;
            }
            public async Task<ServiceRespnse> Handle(SendSnewsTelegramReminderCommand request, CancellationToken cancellationToken)
            {
                var reminders = await _reminderRepository.GetAsNoTrackingQuery().Where(z => z.IsRemindMeAgain && z.RemindAgainDate.Value == DateTime.Now.Date && !z.IsExpire).ToListAsync();

                //telegram config
                var ApiKey = _configuration["TelegramSettings:TelegramKey"];
                reminders.ForEach(z =>
                {
                    TelegramSendMessage(ApiKey, z.TelegramId.ToString(), $"<b> با سلام جهت یادآوری به قیمت {z.Amount.ToString().ToPersianNumber()} \n بابت:{z.Description}</b>");
                    z.IsExpire = true;
                });
                await _reminderRepository.UpdateRangeAsync(reminders,cancellationToken);
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
