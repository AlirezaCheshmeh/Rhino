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
using Application.Services.TelegramServices;

namespace TelegramBot.BaseMethods
{
    public class HandleUpdate : BaseConfig
    {
        private readonly HandleCallbackQuery _handleCallbackQuery;
        private readonly HandleMessage _handleMessage;
        private readonly ICacheServices _cache;
        private readonly IDistributedCache _disCache;
        private readonly IDynamicButtonsServices _dynamicButtonsServices;
        public HandleUpdate(ICacheServices cache, IDistributedCache disCache, IDynamicButtonsServices dynamicButtonsServices)
        {
            _handleCallbackQuery = new(cache, disCache,dynamicButtonsServices);
            _handleMessage = new(cache, disCache,dynamicButtonsServices);
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
                    //handle spesefic commands
                    if (callBackData.Contains(ConstCallBackData.DailyOrSpecificDate.Bank))
                    {
                        commandState = CommandState.Amount;
                        callBackData = callBackData.Substring(0,4);
                    }
                    if (callBackData.Contains(ConstCallBackData.DailyCategory.Category))
                        commandState = CommandState.ChooseBankDaily;

                    else
                        commandState = callBackData switch
                        {
                            ConstCallBackData.Menu.InboundTransaction => CommandState.InsertInboundTransaction,
                            ConstCallBackData.Menu.Settings => CommandState.Settings,
                            ConstCallBackData.Menu.OutboundTransaction => CommandState.InsertOutboundTransaction,
                            ConstCallBackData.BankMenu.GetBankList => CommandState.GetBankList,
                            ConstCallBackData.BankMenu.InsertNewBank => CommandState.InsertNewbank,
                            ConstCallBackData.OutboundTransaction.Daily => CommandState.ChooseCategoryDaily,
                            ConstCallBackData.OutboundTransaction.SpecificDate => CommandState.ChooseBankSpecificDate,
                            ConstCallBackData.DailyOrSpecificDate.Bank => CommandState.Amount,
                            ConstCallBackData.OutboundTransactionPreview.Submit => CommandState.OutBoundTransactionSubmit,
                            ConstCallBackData.Global.Back => userSession.LastCommand,
                            ConstCallBackData.Global.BackToMenu => CommandState.BackToMenu,
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
                        CommandState.InsertNewbank => CommandState.InsertNewbankMessage,
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
            ChooseCategoryDaily,
            ChooseBankDaily,
            ChooseBankSpecificDate,
            Amount,
            Description,
            OutboundTransactionPreview,
            OutBoundTransactionSubmit,
            OutboundTransactionCancel,


            BankInsertPreview,
            Settings,
            InsertNewbank,
            InsertNewbankMessage,
            GetBankList,


            InsertTransaction,
            SignAndAccept,
            Start,
            Date,
            FromDate,
            ToDate,
            Bank,
            LoginBefore,
            AcceptTransaction,
            GetLastTransaction,
            InsertDailyInBound,
            InsertWithDateInBound,
            BankSelect,
            BackToMenu,
           
        }

        public enum BotMessageType
        {
            Text,
            CallbackQuery
        }

    }
}
