using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;
using TelegramBot.Configurations.Base;
using Application.Services.CacheServices;

namespace TelegramBot.BaseMethods
{
    public class HandleUpdate : BaseConfig
    {
        private readonly HandleCallbackQuery _handleCallbackQuery;
        private readonly HandleMessage _handleMessage;
        private readonly ICacheServices _cache;
        private readonly IDistributedCache _disCache;
        public HandleUpdate(ICacheServices cache, IDistributedCache disCache)
        {
            _handleCallbackQuery = new(cache, disCache);
            _handleMessage = new(cache, disCache);
            _cache = cache;
            _disCache = disCache;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken = default)
        {

            try
            {
                string userIdKey = update.Message?.From?.Id.ToString() ?? update.CallbackQuery?.From?.Id.ToString();
                if (string.IsNullOrEmpty(userIdKey))
                {
                    return;
                }
                UserSession? cacheData = null;

                try
                {
                    // Attempt to retrieve data from the cache
                    var data = await _disCache.GetAsync(userIdKey, cancellationToken);

                    // Check if data is null or invalid
                    if (data != null && data.Length > 0)
                    {
                        // Attempt to deserialize the data to an object of type UserSession
                        cacheData = JsonSerializer.Deserialize<UserSession>(data);
                    }
                }
                catch (JsonException ex)
                {
                    await Console.Error.WriteLineAsync($"Failed to deserialize data for key '{userIdKey}': {ex.Message}");
                }

                UserSession? newUserSession = null;


                // Handle different update types
                if (update.Type == UpdateType.Message && update.Message?.Type == MessageType.Text)
                {
                    if (cacheData == null)
                    {
                        newUserSession = new UserSession
                        {
                            Type = BotMessageType.Text,
                            CommnadState = CommandState.Start,
                            LastCommand = "/Start",
                            UserId = Convert.ToInt64(userIdKey)
                        };
                        var json = JsonSerializer.Serialize(newUserSession);
                        var bytes = Encoding.UTF8.GetBytes(json);
                        await _disCache.SetAsync(userIdKey, bytes);
                    }
                    await _handleMessage.HandleMessageAsync(client, update.Message, cacheData ?? newUserSession);
                }
                else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                {
                    if (cacheData == null)
                    {
                        newUserSession = new UserSession
                        {
                            Type = BotMessageType.CallbackQuery,
                            CommnadState = CommandState.SignAndAccept,
                            LastCommand = "SignAndAccept",
                            UserId = Convert.ToInt64(userIdKey)
                        };
                        var json = JsonSerializer.Serialize(newUserSession);
                        var bytes = Encoding.UTF8.GetBytes(json);
                        await _disCache.SetAsync(userIdKey, bytes);

                    }
                    await _handleCallbackQuery.HandleCallbackQueryAsync(client, update.CallbackQuery, cacheData ?? newUserSession);
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }


        public class UserSession
        {
            public BotMessageType Type { get; set; }
            public long UserId { get; set; }
            public string LastCommand { get; set; } = string.Empty;
            public CommandState CommnadState { get; set; }

        }

        public enum CommandState
        {
            InsertTransaction,
            SignAndAccept,
            Start,
            Amount,
            Date,
            FromDate,
            ToDate,
            Description,
            Category,
            Bank,
            loginBefore,
            AcceptTransaction,
            GetLastTransaction,
            Menu,
            Init,
            InsertTransactionInBound,
            InsertDailyInBound,
            InsertWithDateInBound,
            Choosebank,
            BankSelect
        }

        public enum BotMessageType
        {
            Text,
            CallbackQuery
        }

    }
}
