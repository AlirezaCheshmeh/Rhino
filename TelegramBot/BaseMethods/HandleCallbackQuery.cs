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

namespace TelegramBot.BaseMethods
{
    public class HandleCallbackQuery : BaseConfig
    {
        private readonly ICacheServices _cache;
        private readonly IDistributedCache _disCache;

        public HandleCallbackQuery(ICacheServices cache, IDistributedCache disCache)
        {
            _cache = cache;
            _disCache = disCache;
        }
        public async Task HandleCallbackQueryAsync(ITelegramBotClient client, CallbackQuery callbackQuery, UserSession userSession)
        {
            MenuConfigs menu = new(client);
            TransactionConfigs transactionMenu = new(client);
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
                    #region InsertOutBoundTransaction

                    case CommandState.InsertOutboundTransaction:
                        await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        break;
                    case CommandState.OutboundTransactionDaily:
                        await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        break;
                    case CommandState.ChooseBankDaily:
                        var transactionChooseBank = new TransactionDto();
                        var json = JsonSerializer.Serialize(transactionChooseBank);
                        var bytes = Encoding.UTF8.GetBytes(json);
                        await _disCache.SetAsync(userIdKey + ConstKey.Transaction, bytes);
                        await transactionMenu.SendChooseBankAsync(callbackQuery.Message.Chat.Id, callbackQuery.From.Id);
                        break;
                    case CommandState.Amount:
                        //todo : DRY please write generic service for update, get and set cache.
                        TransactionDto? transactionAmount = null;
                        var data = await _disCache.GetAsync(userIdKey + ConstKey.Transaction);
                        if (data is { Length: > 0 })
                            transactionAmount = JsonSerializer.Deserialize<TransactionDto>(data);
                        if (transactionAmount is null) return;//todo : show suitable message 
                        //
                        var bankId = Convert.ToInt64(callbackQuery.Data?.Split("-").LastOrDefault());//todo not found data and exception
                        transactionAmount.BankId = bankId;
                        await CacheExtension.UpdateCacheAsync(_disCache, userIdKey + ConstKey.Transaction, transactionAmount);
                        var inlineKeyboardAmount = new InlineKeyboardMarkup(new[]
                        {
                            new[] {InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back) },
                        });
                        var amountMessage = await client
                            .SendTextMessageAsync(callbackQuery.Message.Chat.Id, text: ConstMessage.InsertAmount, parseMode: ParseMode.Html, replyMarkup: inlineKeyboardAmount);
                        userSession.MessageIds.Add(amountMessage.MessageId);
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        break;
                    case CommandState.OutBoundTransactionSubmit:
                        TransactionDto? transactionSubmitted = null;
                        var value = await _disCache.GetAsync(userIdKey + ConstKey.Transaction);
                        if (value is { Length: > 0 })
                            transactionSubmitted = JsonSerializer.Deserialize<TransactionDto>(value);
                        if (transactionSubmitted is null) return;//todo : show suitable message 
                        await using (ApplicationDataContext context = new())
                        {
                            var newTransaction = new Transaction
                            {
                                Amount = transactionSubmitted.Amount!.Value,
                                Description = transactionSubmitted.Description,
                                BankId = transactionSubmitted.BankId,
                                MessageId = callbackQuery.Message.MessageId,
                                TelegramId = callbackQuery.From.Id
                            };
                            await context.Set<Transaction>().AddAsync(newTransaction);
                            await context.SaveChangesAsync();
                        }
                        var descriptionMessage = await client
                            .SendTextMessageAsync(callbackQuery.Message.Chat.Id, ConstMessage.Success, parseMode: ParseMode.Html);
                        userSession.MessageIds.Add(descriptionMessage.MessageId);
                        Task.Delay(150).Wait();
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        userSession.MessageIds.RemoveAll(i => i == userSession.MessageIds.First());
                        userSession.MessageIds.ForEach(item =>
                        {
                            client.DeleteMessageAsync(callbackQuery.Message.Chat.Id, item);
                        });
                        await _disCache.RemoveAsync(userIdKey + ConstKey.Transaction);
                        break;
                    case CommandState.OutboundTransactionCancel:
                        var cancelMessage = await client
                            .SendTextMessageAsync(callbackQuery.Message.Chat.Id, ConstMessage.Cancel, parseMode: ParseMode.Html);
                        Task.Delay(150).Wait();
                        userSession.MessageIds.Add(cancelMessage.MessageId);
                        userSession.MessageIds.RemoveAll(i => i == userSession.MessageIds.First());
                        foreach (var id in userSession.MessageIds)
                        {
                            try
                            {
                                await client.DeleteMessageAsync(callbackQuery.Message.Chat.Id, id);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }

                        await _disCache.RemoveAsync(userIdKey + ConstKey.Transaction);
                        await _disCache.RemoveAsync(userIdKey + ConstKey.Session);
                        break;
                    #endregion

                    #region InsertInboundTransaction

                    case CommandState.InsertInboundTransaction:
                        await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "inbound");
                        break;

                        #endregion


                        //case CommandState.InsertTransaction:
                        //    session.CommandState = CommandState.Amount;
                        //    await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        //    await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "هزینه را وارد کنید💸 \n برای مثال 120000");
                        //    break;
                        //case CommandState.AcceptTransaction:
                        //    var cacheData = await _disCache.GetAsync($"{userIdKey}-transaction");
                        //    var transaction = JsonSerializer.Deserialize<Transaction>(cacheData);
                        //    await using (ApplicationDataContext context = new())
                        //    {
                        //        await context.Set<Transaction>().AddAsync(transaction);
                        //        await context.SaveChangesAsync();
                        //        await _disCache.RemoveAsync($"{userIdKey}-transaction");
                        //    }

                        //    session.CommandState = CommandState.GetLastTransaction;
                        //    await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);

                        //    await client.SendTextMessageAsync(
                        //    chatId: callbackQuery.Message.Chat.Id,
                        //    text: $"با موفقیت ثبت شد✔️");
                        //    //send menu to user
                        //    await menu.SendMenuToUserAsync(callbackQuery.Message.Chat.Id);

                        //    break;
                        //case CommandState.GetLastTransaction:
                        //    await using (ApplicationDataContext context = new())
                        //    {
                        //        var data = await context.Set<Transaction>().Where(z => z.TelegramId == callbackQuery.From.Id).OrderByDescending(z => z.CreatedAt).FirstOrDefaultAsync();
                        //        await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"مقدار هزینه: {data.Amount} \n تاریخ ثبت: {DateExtension.ConvertToPersianDate(data.CreatedAt.ToString("yyy/MM/dd"))} \n  توضیحات: {data.Description}");
                        //    }
                        //    break;
                        //case CommandState.InsertInboundTransaction:
                        //    await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        //    //session.CommandState = CommandState.ChooseBank;
                        //    //await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        //    break;
                        //case CommandState.InsertDailyInBound:
                        //    await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);

                        //    break;
                        //case CommandState.BankSelect:
                        //    try
                        //    {
                        //        var bankId = Convert.ToInt64(callbackQuery.Data.Split("-").LastOrDefault());
                        //        await CacheExtension.UpdateCacheAsync(_disCache, $"{userIdKey}-bankID", bankId);
                        //        session.CommandState = CommandState.Amount;
                        //        await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        //        await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "هزینه را وارد کنید💸 \n برای مثال 120000");
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        Console.WriteLine(ex);
                        //        throw;
                        //    }

                        //    break;
                        //case CommandState.ChooseBank:
                        //    await transactionMenu.SendChooseBankAsync(callbackQuery.Message.Chat.Id, callbackQuery.From.Id);
                        //    session.CommandState = CommandState.BankSelect;
                        //    await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        //    break;
                        //default:
                        //    break;
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
        public long? CategoryId { get; set; }
        public DateTime PayDateTime { get; set; } = DateTime.Now;
    }
}
