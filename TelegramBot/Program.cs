using Application.Database;
using Domain.DTOs.Shared;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using static Application.Mediator.User.Command.CreateUserCommand;
using Application.Services.CacheServices;
using Microsoft.Extensions.Caching.Distributed;
using TelegramBot.BaseMethods;
using Infrastructure.Database;
using Application.BackgroundServices;
using Application.Mediator.User.Command;
using AutoMapper;
using Application.MapperProfile;
using Application.Services.TelegramServices;
using Application.Utility;
using TelegramBot.ConstVariable;
using TelegramBot.Configurations.Base;

//ensure Database
await using (ApplicationDataContext context = new())
{
    context.Database.EnsureCreated();
}

//mapper
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new MapperProfile());
});
var mapper = config.CreateMapper();

var serviceCollection = new ServiceCollection()
    .AddMediatR(c =>
    {
        c.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    })
    .AddTransient<IRequestHandler<CreateUserCommand, ServiceRespnse>, CreateUserCommand.CreateUserCommandHandler>()
    .AddSingleton(mapper)
    .AddScoped<IUnitOfWork, UnitOfWork>()
    .AddScoped<IDynamicButtonsServices, DynamicButtonsServices>()
    .AddScoped<ICacheServices, CacheServices>()
    .AddHostedService<ConfigJobSchedule>()
    .AddScoped<IServiceProvider, ServiceProvider>()
    .AddScoped<IApplicationDataContext, ApplicationDataContext>()
    .AddStackExchangeRedisCache(options =>
    {
        options.Configuration = "localhost:6379"; // Redis connection string
    })
    .BuildServiceProvider();

var mediator = serviceCollection.GetRequiredService<IMediator>();
var serviceProvider = serviceCollection.GetRequiredService<IServiceProvider>();
var cache = serviceCollection.GetRequiredService<ICacheServices>();
var disCache = serviceCollection.GetRequiredService<IDistributedCache>();
var dynamicButtons = serviceCollection.GetRequiredService<IDynamicButtonsServices>();

CacheExtension.Initialize(disCache);
HandleUpdate handleUpdate = new(cache, disCache,dynamicButtons);
HandleError handleError = new();

var client = new TelegramBotClient(handleUpdate.TelegramKey);
await TelegramBot.Configurations.Commands.Command.SetBotCommands(client);
await client.DeleteWebhookAsync();
using var cts = new CancellationTokenSource();

var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

client.StartReceiving(handleUpdate.HandleUpdateAsync, handleError.HandleerrorAsync, receiverOptions, cts.Token);
var me = await client.GetMeAsync();
Console.WriteLine("bot started....");
Console.WriteLine($"Hello I am {me.Username} I'm here to help you!");
Console.ReadKey();
cts.Cancel();









//hangfire configs
//GlobalConfiguration.Configuration.UseSqlServerStorage("Server=.;Database=Rhino;User Id=sa;Password=1;TrustServerCertificate=True; MultipleActiveResultSets=True;");

//using (var server = new BackgroundJobServer())
//{
//    Console.WriteLine("job started");
//    var targetDate = new DateTime(2020, 01, 01, 1, 0, 0);
//    targetDate = targetDate.ToUniversalTime();
//    try
//    {
//        RecurringJob.AddOrUpdate("VerifyOrderWarranties", () => SendReminderTelegramMessage(), Cron.Daily(Convert.ToInt32(targetDate.ToString("HH")), targetDate.Day));
//        server.Start();
//    }
//    catch (Exception)
//    {

//        throw;
//    }
//}

////scope CQRS
//async Task SendReminderTelegramMessage()
//{
//    using var scope = serviceProvider.CreateScope();
//    var commandDispacher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

//    await commandDispacher.SendAsync(new SendTelegramReminderCommand());
//}

