using Application.Extensions;
using Application.Services.CacheServices;
using Application.Utility;
using Domain.Entities.Transactions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Configurations;
using TelegramBot.Configurations.Base;
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

        //handle callback query
        public async Task HandleCallbackQueryAsync(ITelegramBotClient client, CallbackQuery callbackQuery, UserSession session)
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
                switch (session.CommandState)
                {
                    
                    case CommandState.InsertOutboundTransaction:
                        await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        break;
                    case CommandState.InsertInboundTransaction:
                        await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "inbound");
                        break;
                    case CommandState.OutboundTransactionDaily:
                        await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        break;

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

                //Get Last Transaction
                if (callbackQuery.Data == "GetLastTransaction")
                {

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
}
