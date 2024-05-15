using Application.Services.CacheServices;
using Application.Utility;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Application.Services.TelegramServices;
using Application.Services.TelegramServices.ConstVariable;
using Application.Services.TelegramServices.Configurations.Base;
using Application.Common;
using Domain.Entities.Categories;
using Domain.Entities.Banks;
using Domain.Entities.UserPurchases;
using Domain.Entities.Plans;
using Application.Cqrs.Commands;
using AutoMapper;
using Telegram.Bot.Polling;
using Application.Services.TelegramServices.Interfaces;
using Application.Extensions;

namespace Application.Services.TelegramServices.BaseMethods
{
    public class HandleUpdate : IHandleUpdates,IScopedDependency
    {
        private readonly IHandleCallbackQuery _handleCallbackQuery;
        private readonly IHandleMessage _handleMessage;
        private readonly IDistributedCache _disCache;

        public HandleUpdate(ICacheServices cache, IDistributedCache disCache, IHandleCallbackQuery handleCallbackQuery)
        {
            _disCache = disCache;
            _handleCallbackQuery = handleCallbackQuery;
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

                    //handle spesefic commands start=================================================
                    if (callBackData.Contains(ConstCallBackData.DailyOrSpecificDate.Bank))
                    {
                        commandState = CommandState.Amount;
                        callBackData = callBackData.Substring(0, 4);
                    }
                    else if (callBackData.Contains(ConstCallBackData.DailyCategory.Category))
                    {
                        commandState = CommandState.ChooseBankDaily;
                    }

                    else if (callBackData.Contains(ConstCallBackData.InboundDailyOrSpecificDate.Bank))
                    {
                        commandState = CommandState.InboundAmount;
                        callBackData = callBackData.Substring(0, 4);
                    }
                    else if (callBackData.Contains(ConstCallBackData.InboundDailyCategory.Category))
                    {
                        commandState = CommandState.InboundChooseBankDaily;
                    }
                    //handle spesefic commands end=================================================

                    //handle other states
                    else
                        commandState = callBackData switch
                        {
                            //menu start===================================================================
                            ConstCallBackData.Menu.BuyAccount => CommandState.BuyAccount,
                            ConstCallBackData.Menu.Reports => CommandState.Reports,
                            ConstCallBackData.Menu.PeriodicReminder => CommandState.RemindPeriodic,
                            ConstCallBackData.Menu.InboundTransaction => CommandState.InsertInboundTransaction,
                            ConstCallBackData.Menu.Settings => CommandState.Settings,
                            ConstCallBackData.Menu.OutboundTransaction => CommandState.InsertOutboundTransaction,
                            //menu end===================================================================

                            //bank start====================================================================
                            ConstCallBackData.BankMenu.GetBankList => CommandState.GetBankList,
                            ConstCallBackData.BankMenu.InsertNewBank => CommandState.InsertNewbank,
                            //bank end====================================================================

                            //inbound start=================================================================
                            ConstCallBackData.InboundTransaction.Daily => CommandState.InboundChooseCategoryDaily,
                            ConstCallBackData.InboundDailyCategory.Category => CommandState.InboundChooseBankDaily,
                            ConstCallBackData.InboundTransaction.SpecificDate => CommandState.InboundChooseBankSpecificDate,
                            ConstCallBackData.InboundTransactionPreview.Submit => CommandState.InBoundTransactionSubmit,
                            ConstCallBackData.InboundTransactionPreview.Cancel => CommandState.InboundTransactionCancel,
                            ConstCallBackData.DailyOrSpecificDate.Bank => CommandState.Amount,
                            //inbound end===================================================================

                            //outbound start=================================================================
                            ConstCallBackData.OutboundTransaction.Daily => CommandState.ChooseCategoryDaily,
                            ConstCallBackData.DailyCategory.Category => CommandState.ChooseBankDaily,
                            ConstCallBackData.OutboundTransaction.SpecificDate => CommandState.ChooseBankSpecificDate,
                            ConstCallBackData.OutboundTransactionPreview.Submit => CommandState.OutBoundTransactionSubmit,
                            ConstCallBackData.OutboundTransactionPreview.Cancel => CommandState.OutboundTransactionCancel,
                            ConstCallBackData.InboundDailyOrSpecificDate.Bank => CommandState.InboundAmount,
                            //outbound end===================================================================

                            //global start===================================================================
                            ConstCallBackData.Global.Back => userSession.LastCommand,
                            ConstCallBackData.Global.BackToMenu => CommandState.BackToMenu,
                            //global end===================================================================
                            _ => CommandState.Init
                            //todo : handle init state in call back query 
                        };
                }
                else if (update.Message?.Text is not null)
                {

                    commandState = userSession.CommandState switch
                    {
                        //outbound start========================================================
                        CommandState.Amount => CommandState.Description,
                        CommandState.Description => CommandState.OutboundTransactionPreview,
                        CommandState.OutboundTransactionPreview => CommandState.OutboundTransactionPreview,
                        //outbound end==========================================================

                        //Inbound start========================================================
                        CommandState.InboundAmount => CommandState.InboundDescription,
                        CommandState.InboundDescription => CommandState.InboundTransactionPreview,
                        CommandState.InboundTransactionPreview => CommandState.InboundTransactionPreview,
                        //Inbound end==========================================================

                        //settings =>Bank start================================================
                        CommandState.InsertNewbank => CommandState.InsertNewbankMessage,
                        //settings =>Bank end==================================================

                        _ => CommandState.Init
                    };
                }
                userSession.LastCommand = userSession.CommandState;
                userSession.CommandState = commandState;
                await CacheExtension.UpdateCacheAsync(_disCache, userIdKey + ConstKey.Session, userSession);

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
                //todo: log here
                Console.WriteLine(ex);
            }

        }


        //user session
        public class UserSession
        {
            public long UserId { get; set; }
            public List<int> MessageIds { get; set; } = new();
            public CommandState LastCommand { get; set; } = CommandState.Init;
            public CommandState CommandState { get; set; } = CommandState.Init;
        }

        //command states
        public enum CommandState
        {
            Init,

            //inbound
            InsertInboundTransaction,
            InboundTransactionDaily,
            InboundTransactionSpecificDate,
            InboundChooseCategoryDaily,
            InboundChooseBankDaily,
            InboundChooseBankSpecificDate,
            InboundAmount,
            InboundDescription,
            InboundTransactionPreview,
            InBoundTransactionSubmit,
            InboundTransactionCancel,

            //outbound
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

            //settings
            BankInsertPreview,
            Settings,
            InsertNewbank,
            InsertNewbankMessage,
            GetBankList,

            //transaction
            InsertTransaction,

            //bank
            Bank,
            BankSelect,

            //global
            BackToMenu,

            //features
            BuyAccount,
            Reports,
            RemindPeriodic,
        }

    }
}
