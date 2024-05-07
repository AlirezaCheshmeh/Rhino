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
using Application.Utility;
using TelegramBot.ConstVariable;

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
                var commandState = CommandState.Init;
                var botMessageType = BotMessageType.Text;
                if (update.CallbackQuery is not null)
                {
                    var callBackData = update.CallbackQuery.Data;
                    if (string.IsNullOrEmpty(callBackData))
                        return;
                    commandState = callBackData switch
                    {
                        ConstCallBackData.Menu.InboundTransaction => CommandState.InsertInboundTransaction,
                        ConstCallBackData.Menu.OutboundTransaction => CommandState.InsertOutboundTransaction,
                        ConstCallBackData.OutboundTransaction.Daily => CommandState.OutboundTransactionDaily,
                        _ => CommandState.Init
                    };
                    botMessageType = BotMessageType.CallbackQuery;
                }
                else if (update.Message?.Text is not null)
                {
                    //todo : handle message like query(CallBackData) above 
                }
                var userIdKey = update.Message?.From?.Id.ToString() ?? update.CallbackQuery?.From?.Id.ToString();
                if (string.IsNullOrEmpty(userIdKey))
                    return;
                UserSession? userSession = null;
                try
                {
                    var data = await _disCache.GetAsync(userIdKey, cancellationToken);
                    if (data is { Length: > 0 })
                        userSession = JsonSerializer.Deserialize<UserSession>(data);
                    if (userSession is null)
                    {
                        userSession = new UserSession
                        {
                            Type = botMessageType,
                            CommandState = commandState,
                            UserId = Convert.ToInt64(userIdKey)
                        };
                        var json = JsonSerializer.Serialize(userSession);
                        var bytes = Encoding.UTF8.GetBytes(json);
                        await _disCache.SetAsync(userIdKey, bytes, token: cancellationToken);
                    }
                    else
                    {
                        userSession.CommandState = commandState;
                        await CacheExtension.UpdateCacheAsync(_disCache, userIdKey, userSession);
                    }
                }
                catch (JsonException ex)
                {
                    await Console.Error.WriteLineAsync($"Failed to deserialize data for key '{userIdKey}': {ex.Message}");
                }
                // why not if else???
                switch (update)
                {
                    case { Type: UpdateType.Message, Message.Type: MessageType.Text }:
                        await _handleMessage.HandleMessageAsync(client, update.Message, userSession!);
                        break;
                    case { Type: UpdateType.CallbackQuery, CallbackQuery: not null }:
                        await _handleCallbackQuery.HandleCallbackQueryAsync(client, update.CallbackQuery, userSession!);
                        break;
                }
            }
            catch (Exception ex)
            {
                //todo: set sa
                throw;
            }

        }


        public class UserSession
        {
            public BotMessageType Type { get; set; }
            public long UserId { get; set; }
            public string LastCommand { get; set; } = string.Empty;
            public CommandState CommandState { get; set; }

        }

        public enum CommandState
        {
            Init,
            InsertInboundTransaction,
            InsertOutboundTransaction,
            OutboundTransactionDaily,
            OutboundTransactionSpecificDate,
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
            LoginBefore,
            AcceptTransaction,
            GetLastTransaction,
            InsertDailyInBound,
            InsertWithDateInBound,
            ChooseBank,
            BankSelect
        }

        public enum BotMessageType
        {
            Text,
            CallbackQuery
        }

    }
}
