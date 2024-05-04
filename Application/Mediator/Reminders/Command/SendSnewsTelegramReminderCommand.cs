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

namespace Application.Mediator.Reminders.Command
{
    public class SendSnewsTelegramReminderCommand : ICommand<ServiceRespnse>
    {
        public class SendSnewsTelegramReminderCommandHandler : ICommandHandler<SendTelegramReminderCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<Reminder> _reminderRepository;

            public SendSnewsTelegramReminderCommandHandler(IGenericRepository<Reminder> reminderRepository)
            {
                _reminderRepository = reminderRepository;
            }
            public async Task<ServiceRespnse> Handle(SendTelegramReminderCommand request, CancellationToken cancellationToken)
            {
                var reminders = await _reminderRepository.GetAsNoTrackingQuery().Where(z => z.IsRemindMeAgain && z.RemindAgainDate.Value == DateTime.Now.Date && !z.IsExpire).ToListAsync();

                //telegram config
                var ApiKey = "7103099486:AAGyvP7tji0wMat1NqlDgTJitlsavFtzcGg";
                reminders.ForEach(z =>
                {
                    TelegramSendMessage(ApiKey, z.TelegramId.ToString(), $"با سلام جهت یادآوری به قیمت {z.Amount}");
                });
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
