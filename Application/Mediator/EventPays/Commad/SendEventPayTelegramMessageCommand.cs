using Application.Common;
using Application.Cqrs.Commands;
using Application.Mediator.Reminders.Command;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Domain.DTOs.Shared;
using Domain.Entities.EventPays;

namespace Application.Mediator.EventPays.Commad
{
    public class SendEventPayTelegramMessageCommand : ICommand<ServiceRespnse>
    {
        public class SendEventPayTelegramMessageCommandHandler : ICommandHandler<SendTelegramReminderCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<EventPay> _periodTransactionRepository;

            public SendEventPayTelegramMessageCommandHandler(IGenericRepository<EventPay> periodTransactionRepository)
            {
                _periodTransactionRepository = periodTransactionRepository;
            }
            public async Task<ServiceRespnse> Handle(SendTelegramReminderCommand request, CancellationToken cancellationToken)
            {

                var reminders = await _periodTransactionRepository.GetAsNoTrackingQuery().Where(z => z.PayDate.Date == DateTime.Now.Date.AddDays(-1)).ToListAsync();

                //telegram config
                var ApiKey = "7103099486:AAGyvP7tji0wMat1NqlDgTJitlsavFtzcGg";
                reminders.ForEach(z =>
                {
                    TelegramSendMessage(ApiKey, z.TelegramId.ToString(), $"با سلام جهت یادآوری به قیمت {z.Amount} بابت {z.Description}");
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
