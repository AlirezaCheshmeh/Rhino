using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Domain.Entities.Transactions;
using static TelegramBot.BaseMethods.HandleUpdate;
using Application.Services.CacheServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Application.Utility;
using Microsoft.AspNetCore.Builder;
using System.Collections;
using TelegramBot.Configurations;
using TelegramBot.Configurations.Base;

namespace TelegramBot.BaseMethods
{
    public class HandleMessage : BaseConfig
    {
        private readonly ICacheServices _cahce;
        private readonly IDistributedCache _disCache;

        public HandleMessage(ICacheServices cahce, IDistributedCache disCache)
        {
            _cahce = cahce;
            _disCache = disCache;
        }

        public async Task HandleMessageAsync(ITelegramBotClient client, Message message, UserSession session)
        {
            MenuConfigs Menu = new(client);
            TransactionTelegramConfigs transactionMenu = new(client);
            try
            {
                Console.WriteLine($"GetCache Succed state:{session.CommnadState} User:{message.From.FirstName + " " + message.From.LastName}");
                Console.WriteLine($"message recieved from {message.Chat.FirstName + message.Chat.LastName} Message:{message.Text}");
                var userIdKey = message.From.Id;
                var command = new string[] { "/menu", "/start" };
                if (command.Contains(message.Text))
                {
                    Console.WriteLine("true");
                    await _disCache.RemoveAsync(userIdKey.ToString());
                    session.CommnadState = CommandState.Init;
                    await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                }
                switch (session.CommnadState)
                {

                    case CommandState.Init:
                        await Menu.SendMenuToUserAsync(message.Chat.Id);
                        session.CommnadState = CommandState.InsertTransactionInBound;
                        await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        break;
                    case CommandState.Amount:
                        if (!(IsInteger(message.Text) && IsDouble(message.Text)))
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, "قیمت به درستی وارد نشده است");
                        }
                        else
                        {

                            var cacheData = await _disCache.GetStringAsync($"{userIdKey}-bankID");
                            _ = long.TryParse(cacheData, out var resOfId);
                            //if (resOfId == 0 ) todo : validtion for if not parse to long, please handle.


                            var amount = Convert.ToDecimal(message.Text);
                            var transaction = new Transaction
                            {
                                Amount = amount,
                                BankId = resOfId,
                                CategoryId = 2,
                                Description = "",
                                TelegramId = userIdKey,
                                MessageId = message.MessageId
                            };

                            await CacheExtension.UpdateCacheAsync(_disCache, $"{userIdKey}-transaction", transaction);

                            session.CommnadState = CommandState.Description;
                            session.LastCommand = "Amount";
                            await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                            await client.SendTextMessageAsync(message.Chat.Id, "هزینه با موفقیت ثبت شد✔️ \n توضیحات را وارد کنید📃");

                        }
                        break;
                    case CommandState.Description:

                        try
                        {

                            Transaction cacheData = new();
                            byte[] tranaction = await _disCache.GetAsync($"{userIdKey}-transaction");

                            if (tranaction != null && tranaction.Length > 0)
                            {
                                cacheData = JsonSerializer.Deserialize<Transaction>(tranaction);
                            }
                            cacheData!.Description = message.Text;
                            await CacheExtension.UpdateCacheAsync(_disCache, $"{userIdKey}-transaction", cacheData);
                        }
                        catch (JsonException ex)
                        {
                            Console.Error.WriteLine($"Failed to deserialize data for key '{userIdKey}': {ex.Message}");
                        }

                        var inlineKeyboardDescription = new InlineKeyboardMarkup(new[]
                         {
                            new[]
                            {
                                 InlineKeyboardButton.WithCallbackData("ثبت نهایی", "AcceptTransaction"),
                            },
                        });
                        await client.SendTextMessageAsync(
                       chatId: message.Chat.Id,
                       text: "برای ثبت نهایی روی دکمه زیر بزنید \n توضیحات با موفقیت ثبت شد✔️ ",
                       replyMarkup: inlineKeyboardDescription);

                        session.CommnadState = CommandState.AcceptTransaction;
                        await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);

                        break;
                    case CommandState.InsertTransactionInBound:
                        await transactionMenu.SendInBoundTransactionAsync(message.Chat.Id);
                        session.CommnadState = CommandState.Choosebank;
                        await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        break;
                    case CommandState.InsertDailyInBound:
                        await transactionMenu.SendInBoundTransactionAsync(message.Chat.Id);

                        break;
                    case CommandState.Choosebank:
                        await transactionMenu.SendInBoundTransactionAsync(message.Chat.Id);

                        break;
                    case CommandState.InsertWithDateInBound:
                        session.CommnadState = CommandState.Choosebank;
                        await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        await transactionMenu.SendInBoundTransactionAsync(message.Chat.Id);

                        break;
                    default:
                        break;
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
