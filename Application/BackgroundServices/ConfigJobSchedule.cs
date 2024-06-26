﻿using Application.Cqrs.Commands;
using Application.Mediator.EventPays.Commad;
using Application.Mediator.Reminders.Command;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.BackgroundServices
{
    public class ConfigJobSchedule : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;


        public ConfigJobSchedule(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {

               
                try
                {
                    RecurringJob.AddOrUpdate("SendTelegramRemindMessage", () => SendReminderTelegramMessage(), Cron.Minutely());
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }

        //scope CQRS
        public async Task SendReminderTelegramMessage()
        {
            using var scope = _serviceProvider.CreateScope();
            var commandDispacher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

            await commandDispacher.SendAsync(new SendTelegramReminderCommand());
        }

        //public async Task SendPeriodReminderTelegramMessage()
        //{
        //    using var scope = _serviceProvider.CreateScope();
        //    var commandDispacher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        //    await commandDispacher.SendAsync(new SendEventPayTelegramMessageCommand());
        //}
        //public async Task SnewsSend()
        //{
        //    using var scope = _serviceProvider.CreateScope();
        //    var commandDispacher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        //    await commandDispacher.SendAsync(new SendSnewsTelegramReminderCommand());
        //}
    }
}
