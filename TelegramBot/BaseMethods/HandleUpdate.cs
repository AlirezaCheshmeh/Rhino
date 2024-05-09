using Application.Services.CacheServices;
using Application.Utility;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Configurations.Base;
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
            UserSession? userSession = null;
            try
            {
                await client.DeleteWebhookAsync(dropPendingUpdates: true, cancellationToken: cancellationToken);
                var userIdKey = update.Message?.From?.Id.ToString() ?? update.CallbackQuery?.From?.Id.ToString();
                if (string.IsNullOrEmpty(userIdKey))
                    return;
                var data = await _disCache.GetAsync(userIdKey + ConstKey.Session, cancellationToken);
                if (data is { Length: > 0 })
                    userSession = JsonSerializer.Deserialize<UserSession>(data);
                if (userSession is null)
                {
                    userSession = new UserSession { UserId = Convert.ToInt64(userIdKey) };
                    var json = JsonSerializer.Serialize(userSession);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    await _disCache.SetAsync(userIdKey + ConstKey.Session, bytes, token: cancellationToken);
                }
                var commandState = CommandState.Init;
                if (update.CallbackQuery is not null)
                {
                    var callBackData = update.CallbackQuery.Data;
                    if (string.IsNullOrEmpty(callBackData))
                        return;
                    if (callBackData.Contains(ConstCallBackData.DailyOrSpecificDate.Bank))
                        commandState = CommandState.Amount;
                    else
                        commandState = callBackData switch
                        {
                            ConstCallBackData.Menu.InboundTransaction => CommandState.InsertInboundTransaction,
                            ConstCallBackData.Menu.OutboundTransaction => CommandState.InsertOutboundTransaction,
                            ConstCallBackData.OutboundTransaction.Daily => CommandState.ChooseBankDaily,
                            ConstCallBackData.OutboundTransaction.SpecificDate => CommandState.ChooseBankSpecificDate,
                            ConstCallBackData.OutboundTransactionPreview.Submit => CommandState.OutBoundTransactionSubmit,
                            ConstCallBackData.Global.Back => userSession.LastCommand,
                            ConstCallBackData.OutboundTransactionPreview.Cancel => CommandState.OutboundTransactionCancel,
                            _ => CommandState.Init
                            //todo : handle init state in call back query 
                        };
                }
                else if (update.Message?.Text is not null)
                {
                    commandState = userSession.CommandState switch
                    {
                        CommandState.Amount => CommandState.Description,
                        CommandState.Description => CommandState.OutboundTransactionPreview,
                        _ => CommandState.Init
                    };
                }
                userSession.LastCommand = userSession.CommandState;
                userSession.CommandState = commandState;
                await CacheExtension.UpdateCacheAsync(_disCache, userIdKey + ConstKey.Session, userSession);

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
            public long UserId { get; set; }
            public List<int> MessageIds { get; set; } = new();
            public CommandState LastCommand { get; set; } = CommandState.Init;
            public CommandState CommandState { get; set; } = CommandState.Init;
        }

        public enum CommandState
        {
            Init,
            InsertInboundTransaction,
            InsertOutboundTransaction,
            OutboundTransactionDaily,
            OutboundTransactionSpecificDate,
            ChooseBankDaily,
            ChooseBankSpecificDate,
            Amount,
            Description,
            OutboundTransactionPreview,
            OutBoundTransactionSubmit,
            OutboundTransactionCancel,

            InsertTransaction,
            SignAndAccept,
            Start,
            Date,
            FromDate,
            ToDate,
            Category,
            Bank,
            LoginBefore,
            AcceptTransaction,
            GetLastTransaction,
            InsertDailyInBound,
            InsertWithDateInBound,
            BankSelect
        }

        public enum BotMessageType
        {
            Text,
            CallbackQuery
        }

    }
}
