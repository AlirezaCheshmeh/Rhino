using Application.Services.TelegramServices.BaseMethods;
using Application.Utility;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Polling;
using Telegram.Bot;
using Microsoft.Extensions.Caching.Distributed;
using Application.Services.CacheServices;
using Application.Services.TelegramServices;
using Application.Common;
using Application.Cqrs.Commands;
using AutoMapper;
using Domain.Entities.Banks;
using Domain.Entities.Categories;
using Domain.Entities.Plans;
using Domain.Entities.UserPurchases;
using Application.Services.TelegramServices.Configurations.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Application.Database;
using MediatR;
using Application.Services.TelegramServices.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.BackgroundServices
{
    public class TelegramJobSchedule : BackgroundService
    {
        private readonly IHandleUpdates _handleUpdates;
        private readonly IDistributedCache _distributedCache;
        private readonly IConfiguration _configuration;

        public TelegramJobSchedule(IHandleUpdates handleUpdates, IDistributedCache distributedCache, IConfiguration configuration)
        {
            _handleUpdates = handleUpdates;
            _distributedCache = distributedCache;
            _configuration = configuration;

        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            CacheExtension.Initialize(_distributedCache);
            HandleError handleError = new();

            var client = new TelegramBotClient(_configuration["TelegramSettings:TelegramKey"]);
            await Command.SetBotCommands(client);
            await client.DeleteWebhookAsync();
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

            client.StartReceiving(_handleUpdates.HandleUpdateAsync, handleError.HandleerrorAsync, receiverOptions, cts.Token);
            var me = await client.GetMeAsync();
        }
    }
}
