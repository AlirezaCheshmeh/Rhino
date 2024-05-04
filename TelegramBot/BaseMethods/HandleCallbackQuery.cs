using Application.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Domain.Entities.Transactions;
using Microsoft.Extensions.Configuration;
using static TelegramBot.BaseMethods.HandleUpdate;
using Application.Services.CacheServices;
using Application.Utility;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TelegramBot.Configurations;
using TelegramBot.Configurations.Base;
using Infrastructure.Database;

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
            TransactionTelegramConfigs transactionMenu = new(client);
            try
            {
                var userIdKey = callbackQuery.From.Id;

                switch (session.CommnadState)
                {

                    case CommandState.InsertTransaction:
                        session.CommnadState = CommandState.Amount;
                        await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "هزینه را وارد کنید💸 \n برای مثال 120000");
                        break;
                    case CommandState.AcceptTransaction:
                        var cacheData = await _disCache.GetAsync($"{userIdKey}-transaction");
                        var transaction = JsonSerializer.Deserialize<Transaction>(cacheData);
                        using (ApplicationDataContext context = new())
                        {
                            await context.Set<Transaction>().AddAsync(transaction);
                            await context.SaveChangesAsync();
                            await _disCache.RemoveAsync($"{userIdKey}-transaction");
                        }

                        session.CommnadState = CommandState.GetLastTransaction;
                        await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);

                        await client.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: $"با موفقیت ثبت شد✔️");
                        //send menu to user
                        await menu.SendMenuToUserAsync(callbackQuery.Message.Chat.Id);

                        break;
                    case CommandState.GetLastTransaction:
                        using (ApplicationDataContext context = new())
                        {
                            var data = await context.Set<Transaction>().Where(z => z.TelegramId == callbackQuery.From.Id).OrderByDescending(z => z.CreatedAt).FirstOrDefaultAsync();
                            await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"مقدار هزینه: {data.Amount} \n تاریخ ثبت: {DateExtension.ConvertToPersianDate(data.CreatedAt.ToString("yyy/MM/dd"))} \n  توضیحات: {data.Description}");
                        }
                        break;
                    case CommandState.InsertTransactionInBound:
                        await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        session.CommnadState = CommandState.Choosebank;
                        await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        break;
                    case CommandState.InsertDailyInBound:
                        await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);

                        break;
                    case CommandState.BankSelect:
                        try
                        {
                            var bankId = Convert.ToInt64(callbackQuery.Data.Split("-").LastOrDefault());
                            await CacheExtension.UpdateCacheAsync(_disCache, $"{userIdKey}-bankID", bankId);
                            session.CommnadState = CommandState.Amount;
                            await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                            await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "هزینه را وارد کنید");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            throw;
                        }

                        break;
                    case CommandState.Choosebank:
                        await transactionMenu.SendChooseBankAsync(callbackQuery.Message.Chat.Id, callbackQuery.From.Id);
                        session.CommnadState = CommandState.BankSelect;
                        await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        break;
                    default:
                        break;
                }

                //Get Last Tranaction
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
