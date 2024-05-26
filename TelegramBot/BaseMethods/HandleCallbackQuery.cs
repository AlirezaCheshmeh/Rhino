using Application.Services.CacheServices;
using Application.Utility;
using Domain.Entities.Transactions;
using Domain.Enums;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using Infrastructure.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Configurations;
using TelegramBot.Configurations.Base;
using TelegramBot.ConstMessages;
using TelegramBot.ConstVariable;
using static TelegramBot.BaseMethods.HandleUpdate;
using Application.Services.TelegramServices;
using TelegramBot.Configurations.Commands;
using Infrastructure.Configurations.UserPerchases;
using Domain.Entities.UserPurchases;
using Microsoft.EntityFrameworkCore;

namespace TelegramBot.BaseMethods
{
    public class HandleCallbackQuery : BaseConfig
    {
        private readonly ICacheServices _cache;
        private readonly IDistributedCache _disCache;
        private readonly IDynamicButtonsServices _dynamicButtonServices;

        public HandleCallbackQuery(ICacheServices cache, IDistributedCache disCache, IDynamicButtonsServices dynamicButtonServices)
        {
            _cache = cache;
            _disCache = disCache;
            _dynamicButtonServices = dynamicButtonServices;
        }
        public async Task HandleCallbackQueryAsync(ITelegramBotClient client, CallbackQuery callbackQuery, UserSession userSession)
        {
            MenuConfigs menu = new(client, _disCache);
            TransactionConfigs transactionMenu = new(client, _dynamicButtonServices);
            SettingMenuConfigs settingMenu = new(client);
            CategoryConfigs categoryMenu = new(client, _dynamicButtonServices);
            GlobalConfigs globalMessage = new(client);
            PlanConfigs planConfig = new(client);
            AccountConfigs accountConfig = new();
            try
            {
                var userIdKey = callbackQuery.From.Id;
                if (callbackQuery.Message is null)
                {
                    // todo : please show send suitable message for client and write log 
                    return;
                }

                if (userSession.CommandState is not CommandState.Init)
                {
                    userSession.MessageIds.Add(callbackQuery.Message.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                }
                switch (userSession.CommandState)
                {

                    //global start===================
                    #region BackToMenu
                    case CommandState.BackToMenu:
                        await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        break;
                    #endregion
                    //global end=====================


                    //account start==================
                    #region BuyAccount
                    case CommandState.BuyAccount:
                        //await planConfig.SendPlanToUser(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion
                    //account end====================

                    #region outbound
                    //outbound start=================
                    #region InsertOutBoundTransaction

                    case CommandState.InsertOutboundTransaction:
                        await transactionMenu.SendOutBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region OutboundTransactionDaily
                    case CommandState.OutboundTransactionDaily:
                        await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region ChooseCategoryDaily
                    case CommandState.ChooseCategoryDaily:
                        var transaction = new TransactionDto();
                        transaction.Type = TransactionType.OutBound;
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, transaction);
                        //await categoryMenu.SendOutBoundCategoriesToUser(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region ChooseBankDaily
                    case CommandState.ChooseBankDaily:
                        var cacheDate = await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        var catId = Convert.ToInt64(callbackQuery.Data?.Split("-").LastOrDefault());//todo: not found data and exception
                        if (cacheDate is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        else
                        {
                            cacheDate.CategoryId = catId;
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, cacheDate);
                           // await transactionMenu.SendChooseBankAsync(callbackQuery.Message.Chat.Id, callbackQuery.From.Id);
                        }

                        break;
                    #endregion

                    #region Amount
                    case CommandState.Amount:
                        var cacheDateAmount = await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        var bankId = Convert.ToInt64(callbackQuery.Data?.Split("-").LastOrDefault());//todo not found data and exception
                        if (cacheDateAmount is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        cacheDateAmount!.BankId = bankId;
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, cacheDateAmount);
                        var inlineKeyboardAmount = new InlineKeyboardMarkup(new[]
                        {
                            new[] {InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu) },
                        });
                        var amountMessage = await client
                            .SendTextMessageAsync(callbackQuery.Message.Chat.Id, text: ConstMessage.InsertAmount, parseMode: ParseMode.Html, replyMarkup: inlineKeyboardAmount);
                        userSession.MessageIds.Add(amountMessage.MessageId);
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        break;
                    #endregion

                    #region OutBoundTransactionSubmit
                    case CommandState.OutBoundTransactionSubmit:
                        TransactionDto? transactionSubmitted = null;
                        var value = await _disCache.GetAsync(userIdKey + ConstKey.Transaction);
                        if (value is { Length: > 0 })
                            transactionSubmitted = JsonSerializer.Deserialize<TransactionDto>(value);
                        if (transactionSubmitted is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        else
                        {
                            //await using (ApplicationDataContext context = new())
                            //{
                            //    var newTransaction = new Transaction
                            //    {
                            //        Amount = transactionSubmitted.Amount!.Value,
                            //        Type = TransactionType.OutBound,
                            //        Description = transactionSubmitted.Description,
                            //        BankId = transactionSubmitted.BankId,
                            //        MessageId = callbackQuery.Message.MessageId,
                            //        TelegramId = callbackQuery.From.Id
                            //    };
                            //    await context.Set<Transaction>().AddAsync(newTransaction);
                            //    await context.SaveChangesAsync();
                            //}
                            var descriptionMessage = await client
                                .SendTextMessageAsync(callbackQuery.Message.Chat.Id, ConstMessage.Success, parseMode: ParseMode.Html);
                            userSession.MessageIds.Add(descriptionMessage.MessageId);
                            Task.Delay(150).Wait();
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);

                        }
                        break;
                    #endregion

                    #region OutboundTransactionCancel
                    case CommandState.OutboundTransactionCancel:
                        var cancelMessage = await client
                            .SendTextMessageAsync(callbackQuery.Message.Chat.Id, ConstMessage.Cancel, parseMode: ParseMode.Html);
                        Task.Delay(150).Wait();
                        userSession.MessageIds.Add(cancelMessage.MessageId);
                        await _disCache.RemoveAsync(userIdKey + ConstKey.Transaction);
                        await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        break;
                    #endregion
                    //outbound end========================
                    #endregion

                    #region Inbound
                    //inbound start=======================
                    #region InsertInboundTransaction

                    case CommandState.InsertInboundTransaction:
                        await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        break;

                    #endregion

                    #region IntboundTransactionDaily
                    case CommandState.InboundTransactionDaily:
                        await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region InboundChooseCategoryDaily
                    case CommandState.InboundChooseCategoryDaily:
                        var Inboundtransaction = new TransactionDto();
                        Inboundtransaction.Type = TransactionType.InBound;
                        var res = await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, Inboundtransaction);
                       // await categoryMenu.SendInBoundCategoriesToUser(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region InboundChooseBankDaily
                    case CommandState.InboundChooseBankDaily:
                        var InboundcacheDate = await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        var InboundcatId = Convert.ToInt64(callbackQuery.Data?.Split("-").LastOrDefault());//todo: not found data and exception
                        if (InboundcacheDate is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        else
                        {
                            InboundcacheDate.CategoryId = InboundcatId;
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, InboundcacheDate);
                            //await transactionMenu.SendInBoundChooseBankAsync(callbackQuery.Message.Chat.Id, callbackQuery.From.Id);
                        }

                        break;
                    #endregion

                    #region InboundAmount
                    case CommandState.InboundAmount:
                        var InboundcacheDateAmount = await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        var InboundbankId = Convert.ToInt64(callbackQuery.Data?.Split("-").LastOrDefault());//todo not found data and exception
                        if (InboundcacheDateAmount is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        InboundcacheDateAmount!.BankId = InboundbankId;
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, InboundcacheDateAmount);
                        var InboundinlineKeyboardAmount = new InlineKeyboardMarkup(new[]
                        {
                            new[] {InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu) },
                        });
                        var InboundamountMessage = await client
                            .SendTextMessageAsync(callbackQuery.Message.Chat.Id, text: ConstMessage.InsertAmount, parseMode: ParseMode.Html, replyMarkup: InboundinlineKeyboardAmount);
                        userSession.MessageIds.Add(InboundamountMessage.MessageId);
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        break;
                    #endregion

                    #region InboundTransactionSubmit
                    case CommandState.InBoundTransactionSubmit:
                        TransactionDto? InboundtransactionSubmitted = null;
                        var Inboundvalue = await _disCache.GetAsync(userIdKey + ConstKey.Transaction);
                        if (Inboundvalue is { Length: > 0 })
                            InboundtransactionSubmitted = JsonSerializer.Deserialize<TransactionDto>(Inboundvalue);
                        if (InboundtransactionSubmitted is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        else
                        {

                            //await using (ApplicationDataContext context = new())
                            //{
                            //    var newTransaction = new Transaction
                            //    {
                            //        Amount = InboundtransactionSubmitted.Amount!.Value,
                            //        Type = InboundtransactionSubmitted.Type,
                            //        Description = InboundtransactionSubmitted.Description,
                            //        BankId = InboundtransactionSubmitted.BankId,
                            //        MessageId = callbackQuery.Message.MessageId,
                            //        TelegramId = callbackQuery.From.Id,
                            //    };
                            //    await context.Set<Transaction>().AddAsync(newTransaction);
                            //    await context.SaveChangesAsync();
                            //}
                            var InbounddescriptionMessage = await client
                                .SendTextMessageAsync(callbackQuery.Message.Chat.Id, ConstMessage.Success, parseMode: ParseMode.Html);
                            userSession.MessageIds.Add(InbounddescriptionMessage.MessageId);
                            Task.Delay(150).Wait();
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        break;
                    #endregion

                    #region InboundTransactionCancel
                    case CommandState.InboundTransactionCancel:
                        var InboundcancelMessage = await client
                            .SendTextMessageAsync(callbackQuery.Message.Chat.Id, ConstMessage.Cancel, parseMode: ParseMode.Html);
                        Task.Delay(150).Wait();
                        userSession.MessageIds.Add(InboundcancelMessage.MessageId);
                        await _disCache.RemoveAsync(userIdKey + ConstKey.Transaction);
                        await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        break;
                    #endregion
                    //inbound end=========================
                    #endregion


                    //settings start======================
                    #region Settings

                    case CommandState.Settings:
                        await settingMenu.SendSettingMenuToUser(callbackQuery.Message.Chat.Id);
                        userSession.MessageIds.Add(callbackQuery.Message.MessageId);
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        break;

                    #endregion

                    #region InsertNewBank
                    case CommandState.InsertNewbank:
                        var inlineKeyboardInsertBank = new InlineKeyboardMarkup(new[]
                        {
                            new[] {InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back) },
                        });
                        var bankMessage = await client
                            .SendTextMessageAsync(callbackQuery.Message.Chat.Id, text: ConstMessage.InsertNewBank, parseMode: ParseMode.Html, replyMarkup: inlineKeyboardInsertBank);
                        //add message id for delete rollback
                        userSession.MessageIds.Add(callbackQuery.Message.MessageId);
                        userSession.MessageIds.Add(bankMessage.MessageId);
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        break;
                    #endregion
                    //settings end========================



                    //features start=====================
                    #region Reports
                    case CommandState.Reports:
                        var isActive = await accountConfig.CheckUserActiveAccount(userIdKey);
                        if (!isActive)
                        {
                            var message = await globalMessage.SendYouDontHaveActiveAccount(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(message.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        }
                        else
                        {
                            await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "report");
                        }
                        break;
                    #endregion

                    #region PeriodicReminder
                    case CommandState.RemindPeriodic:
                        var IsActiveAccount = await accountConfig.CheckUserActiveAccount(userIdKey);
                        if (!IsActiveAccount)
                        {
                            var message = await globalMessage.SendYouDontHaveActiveAccount(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(message.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        }
                        else
                        {
                            await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "remind");
                        }
                        break;
                        #endregion
                        //features end=====================


                }


                // Acknowledge the callback query to prevent any repeat calls
                await client.AnswerCallbackQueryAsync(callbackQuery.Id);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

        }
    }

    public class TransactionDto
    {
        public decimal? Amount { get; set; }
        public TransactionType? Type { get; set; }
        public string? Description { get; set; }
        public long? TelegramId { get; set; }
        public long? MessageId { get; set; }
        public long? BankId { get; set; }
        public string? BankName { get; set; }
        public long? CategoryId { get; set; }
        public DateTime PayDateTime { get; set; } = DateTime.Now;
    }
}
