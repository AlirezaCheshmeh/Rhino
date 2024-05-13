using Application.Extensions;
using Application.Services.CacheServices;
using Application.Services.TelegramServices;
using Application.Utility;
using Domain.Entities.Banks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Runtime.CompilerServices;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Configurations;
using TelegramBot.Configurations.Base;
using TelegramBot.ConstMessages;
using TelegramBot.ConstVariable;
using static TelegramBot.BaseMethods.HandleUpdate;

namespace TelegramBot.BaseMethods
{
    public class HandleMessage : BaseConfig
    {
        private readonly ICacheServices _cache;
        private readonly IDistributedCache _disCache;
        private readonly IDynamicButtonsServices _dynamicButtonsServices;

        public HandleMessage(ICacheServices cache, IDistributedCache disCache, IDynamicButtonsServices dynamicButtonsServices)
        {
            _cache = cache;
            _disCache = disCache;
            _dynamicButtonsServices = dynamicButtonsServices;
        }

        public async Task HandleMessageAsync(ITelegramBotClient client, Message message, UserSession userSession)
        {
            MenuConfigs menu = new(client, _disCache);
            TransactionConfigs transactionMenu = new(client, _dynamicButtonsServices);
            CategoryConfigs catMenu = new(client, _dynamicButtonsServices);
            GlobalConfigs globalMessage = new(client);
            try
            {
                if (message.From is null)
                    return;
                var userIdKey = message.From.Id;
                var command = new[] { "/menu", "/start" };
                if (command.Contains(message.Text))
                {
                    userSession.CommandState = CommandState.Init;
                    userSession.MessageIds.Add(message.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                    await _disCache.RemoveAsync(userIdKey + ConstKey.Session);
                }
                else if (message.Text == "/intro")
                    await client.SendTextMessageAsync(message.Chat.Id, ConstMessage.Introduction);
                else
                {
                    userSession.MessageIds.Add(message.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                }
                switch (userSession.CommandState)
                {
                    #region Init
                    case CommandState.Init:
                        await menu.SendMenuToUserAsync(message.Chat.Id);
                        break;
                    #endregion

                    //OutBound start==================================
                    #region Description
                    case CommandState.Description:
                        TransactionDto? transactionAmount = null;
                        transactionAmount =
                            await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        //convert to english numbers
                        var amountResult = NumbersConvertorExtension.ToEnglishNumber(message.Text);
                        if (transactionAmount is null)
                        {
                            await globalMessage.SendErrorToUser(message.Chat.Id);
                            await menu.RollBackToMenu(userIdKey, message.Chat.Id, userSession);
                        }
                        if (!(IsInteger(amountResult) && IsDouble(amountResult)))
                        {
                            Message ErrorMessage = await globalMessage.SendAmountValidationErrorMessageToUser(message.Chat.Id);
                            userSession.MessageIds.Add(message.MessageId);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            userSession.CommandState = CommandState.Amount;
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        }
                        else
                        {
                            _ = decimal.TryParse(amountResult, out var amount);
                            transactionAmount!.Amount = amount;
                            await CacheExtension.UpdateCacheAsync(_disCache, userIdKey + ConstKey.Transaction, transactionAmount);

                            var inlineKeyboardDescription = new InlineKeyboardMarkup(new[]
                                 {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back)
                                },
                            });
                            var descriptionMessage = await client
                                .SendTextMessageAsync(message.Chat.Id, ConstMessage.InsertDescription, parseMode: ParseMode.Html,
                                    replyMarkup: inlineKeyboardDescription);
                            userSession.MessageIds.Add(descriptionMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        }
                        break;
                    #endregion

                    #region OutboundTransactionPreview
                    case CommandState.OutboundTransactionPreview:
                        var transaction = await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        if (transaction is null)
                        {
                            await globalMessage.SendErrorToUser(message.Chat.Id);
                            await menu.RollBackToMenu(userIdKey, message.Chat.Id, userSession);
                        }
                        else
                        {

                            transaction.Description = message.Text;
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, transaction);
                            await transactionMenu.SendPreviewAsync(message.Chat.Id, transaction);
                        }
                        break;
                    #endregion
                    //OutBound end=====================================


                    //Inbound start==================================
                    #region InboundDescription
                    case CommandState.InboundDescription:
                        TransactionDto? InboundtransactionAmount = null;
                        InboundtransactionAmount =
                            await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        //convert to english numbers
                        var InboundamountResult = NumbersConvertorExtension.ToEnglishNumber(message.Text);
                        if (InboundtransactionAmount is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, message.Chat.Id, userSession);
                        }
                        if (!(IsInteger(InboundamountResult) && IsDouble(InboundamountResult)))
                        {
                            Message ErrorMessage = await globalMessage.SendAmountValidationErrorMessageToUser(message.Chat.Id);
                            userSession.MessageIds.Add(message.MessageId);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            userSession.CommandState = CommandState.InboundAmount;
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        }
                        else
                        {
                            _ = decimal.TryParse(InboundamountResult, out var amount);
                            InboundtransactionAmount!.Amount = amount;
                            await CacheExtension.UpdateCacheAsync(_disCache, userIdKey + ConstKey.Transaction, InboundtransactionAmount);

                            var inlineKeyboardDescription = new InlineKeyboardMarkup(new[]
                                 {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back)
                                },
                            });
                            var descriptionMessage = await client
                                .SendTextMessageAsync(message.Chat.Id, ConstMessage.InsertDescription, parseMode: ParseMode.Html,
                                    replyMarkup: inlineKeyboardDescription);
                            userSession.MessageIds.Add(descriptionMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        }
                        break;
                    #endregion

                    #region InboundTransactionPreview
                    case CommandState.InboundTransactionPreview:
                        var Inboundtransaction = await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        if (Inboundtransaction is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, message.Chat.Id, userSession);
                        }
                        else
                        {

                            Inboundtransaction.Description = message.Text;
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, Inboundtransaction);
                            await transactionMenu.SendPreviewAsync(message.Chat.Id, Inboundtransaction);
                        }
                        break;
                    #endregion
                    //Inbound end=====================================


                    //settings => Bank start========================
                    #region InsertNewBankMessage
                    case CommandState.InsertNewbankMessage:
                        var bank = new Bank
                        {
                            Branch = "",
                            Name = message.Text,
                            SVG = "",
                            TelegramId = message.From.Id
                        };
                        using (Infrastructure.Database.ApplicationDataContext context = new())
                        {
                            await context.Set<Bank>().AddAsync(bank);
                            await context.SaveChangesAsync();
                        }
                        var BankInsertedMessage = await client
                           .SendTextMessageAsync(message.Chat.Id, ConstMessage.Success, parseMode: ParseMode.Html);
                        userSession.MessageIds.Add(BankInsertedMessage.MessageId);
                        Task.Delay(150).Wait();
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        await menu.RollBackToMenu(userIdKey, message.Chat.Id, userSession);
                        break;
                        #endregion
                        //settings => Bank end==========================
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        //validationTextMessage
        private bool IsInteger(string input)
        {
            return int.TryParse(input, out _);
        }

        // Method to check if a string is a valid double
        private bool IsDouble(string input)
        {
            return double.TryParse(input, out _);
        }

    }
}
