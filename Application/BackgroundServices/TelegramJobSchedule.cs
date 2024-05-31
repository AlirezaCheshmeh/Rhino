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
using Microsoft.Extensions.Logging;
using static System.Net.WebRequestMethods;

namespace Application.BackgroundServices
{
    public class TelegramJobSchedule : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;
        private readonly IServiceScopeFactory _serviceProvider;
        private readonly ILogger<TelegramJobSchedule> _logger;


        public TelegramJobSchedule(IDistributedCache distributedCache, IServiceScopeFactory serviceProvider, IConfiguration configuration, ILogger<TelegramJobSchedule> logger)
        {
            _distributedCache = distributedCache;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool hasRun = false;
            using var scope = _serviceProvider.CreateScope();

            while (true)
            {

                try
                {
                    if (!hasRun)
                    {
                        var _handleUpdates = scope.ServiceProvider.GetRequiredService<IHandleUpdates>();
                        CacheExtension.Initialize(_distributedCache);
                        HandleError handleError = new();
                        var _telegramBotClient = new TelegramBotClient(_configuration["TelegramSettings:TelegramKey"]);
                        await Command.SetBotCommands(_telegramBotClient);
                        await _telegramBotClient.DeleteWebhookAsync();
                        using var cts = new CancellationTokenSource();

                        var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

                        _telegramBotClient.StartReceiving(_handleUpdates.HandleUpdateAsync, handleError.HandleerrorAsync, receiverOptions, cts.Token);
                        _logger.LogInformation("bot start recieving");
                        var me = await _telegramBotClient.GetMeAsync();
                        _logger.LogInformation($"bot start name:{me}");
                        hasRun = true;
                    }

                }
                catch (Exception ex)
                {
                    hasRun = false;
                    Console.WriteLine(ex);
                }


            }

        }
    }
}
