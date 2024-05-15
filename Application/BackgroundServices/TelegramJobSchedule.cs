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
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDistributedCache _distributedCache;


        public TelegramJobSchedule(IHandleUpdates handleUpdates, IDistributedCache distributedCache, ITelegramBotClient telegramBotClient)
        {
            _handleUpdates = handleUpdates;
            _distributedCache = distributedCache;
            _telegramBotClient = telegramBotClient;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            CacheExtension.Initialize(_distributedCache);
            HandleError handleError = new();

            await Command.SetBotCommands(_telegramBotClient);
            await _telegramBotClient.DeleteWebhookAsync();
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

            _telegramBotClient.StartReceiving(_handleUpdates.HandleUpdateAsync, handleError.HandleerrorAsync, receiverOptions, cts.Token);
            var me = await _telegramBotClient.GetMeAsync();
        }
    }
}
