﻿using Application.Common;
using Application.Cqrs.Commands;
using Application.Extensions;
using Application.Mediator.Banks.Command;
using Application.Mediator.Banks.DTOs;
using Application.Services.CacheServices;
using Application.Services.TelegramServices.Configurations;
using Application.Services.TelegramServices.Configurations.Base;
using Application.Services.TelegramServices.ConstVariable;
using Application.Utility;
using AutoMapper;
using Domain.Entities.Banks;
using Domain.Entities.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Application.Services.TelegramServices.BaseMethods.HandleUpdate;

namespace Application.Services.TelegramServices.BaseMethods
{
    public class HandleMessage : BaseConfig
    {
        private readonly ICacheServices _cache;
        private readonly IDistributedCache _disCache;
        private readonly IDynamicButtonsServices _dynamicButtonsServices;
        private readonly IMapper _mapper;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IGenericRepository<Bank> _bankRepo;
        private readonly IGenericRepository<Category> _categoryRepo;
        public HandleMessage(ICacheServices cache, IDistributedCache disCache, IDynamicButtonsServices dynamicButtonsServices, IMapper mapper, ICommandDispatcher commandDispatcher, IGenericRepository<Bank> bankRepo, IGenericRepository<Category> categoryRepo)
        {
            _cache = cache;
            _disCache = disCache;
            _dynamicButtonsServices = dynamicButtonsServices;
            _mapper = mapper;
            _commandDispatcher = commandDispatcher;
            _bankRepo = bankRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task HandleMessageAsync(ITelegramBotClient client, Message message, UserSession userSession)
        {
            MenuConfigs menu = new(client, _disCache);
            TransactionConfigs transactionMenu = new(client, _dynamicButtonsServices,_bankRepo,_categoryRepo);
            CategoryConfigs catMenu = new(client, _dynamicButtonsServices,_categoryRepo);
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
                        var amountResult = message.Text.ToEnglishNumber();
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
                        var InboundamountResult = message.Text.ToEnglishNumber();
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

                        //insert to db
                        var mappedBank = _mapper.Map<BankDTO>(new Bank
                        {
                            Branch = "",
                            Name = message.Text,
                            SVG = "",
                            TelegramId = message.From.Id
                        });
                        await _commandDispatcher.SendAsync(new InsertBankCommand { dto = mappedBank },cancellationToken:default);

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