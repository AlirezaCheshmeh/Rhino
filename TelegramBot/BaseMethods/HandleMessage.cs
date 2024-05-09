using Application.Services.CacheServices;
using Application.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
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

        public HandleMessage(ICacheServices cache, IDistributedCache disCache)
        {
            _cache = cache;
            _disCache = disCache;
        }

        public async Task HandleMessageAsync(ITelegramBotClient client, Message message, UserSession userSession)
        {
            MenuConfigs menu = new(client);
            TransactionConfigs transactionMenu = new(client);
            try
            {
                if (message.From is null)
                    return;
                var userIdKey = message.From.Id;
                var command = new[] { "/menu", "/start" };
                if (command.Contains(message.Text))
                {
                    userSession.CommandState = CommandState.Init;
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
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
                    case CommandState.Init:
                        await menu.SendMenuToUserAsync(message.Chat.Id);
                        break;
                    case CommandState.Description:
                        TransactionDto? transactionAmount = null;
                        transactionAmount =
                            await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        if (transactionAmount is null) return;//todo : show suitable message 
                        if (!(IsInteger(message.Text) && IsDouble(message.Text)))
                            return; // todo : send suitable message / write global message for validation.
                        _ = long.TryParse(message.Text, out var amount);
                        transactionAmount.Amount = amount;
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
                        break;
                    case CommandState.OutboundTransactionPreview:
                        var transaction = await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        if (transaction is null) return; // todo : show suitable message to client.
                        transaction.Description = message.Text;
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, transaction);
                        await transactionMenu.SendPreviewAsync(message.Chat.Id, transaction);
                        break;
                        //case CommandState.InsertInboundTransaction:
                        //    await transactionMenu.SendInBoundTransactionAsync(message.Chat.Id);
                        //    session.CommandState = CommandState.ChooseBank;
                        //    await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        //    break;

                        //case CommandState.Amount:
                        //    if (!(IsInteger(message.Text) && IsDouble(message.Text)))
                        //    {
                        //        await client.SendTextMessageAsync(message.Chat.Id, " ⚠️ قیمت به درستی وارد نشده است \n دوباره وارد کنید");
                        //    }
                        //    else
                        //    {

                        //        var cacheData = await _disCache.GetStringAsync($"{userIdKey}-bankID");
                        //        _ = long.TryParse(cacheData, out var resOfId);
                        //        //if (resOfId == 0 ) todo : validation for if not parse to long, please handle.


                        //        var amount = Convert.ToDecimal(message.Text);
                        //        var transaction = new Transaction
                        //        {
                        //            Amount = amount,
                        //            BankId = resOfId,
                        //            CategoryId = 2,
                        //            Description = "",
                        //            TelegramId = userIdKey,
                        //            MessageId = message.MessageId
                        //        };

                        //        await CacheExtension.UpdateCacheAsync(_disCache, $"{userIdKey}-transaction", transaction);

                        //        session.CommandState = CommandState.Description;
                        //        session.LastCommand = "Amount";
                        //        await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        //        await client.SendTextMessageAsync(message.Chat.Id, "هزینه با موفقیت ثبت شد✔️ \n توضیحات را وارد کنید📃");

                        //    }
                        //    break;
                        //case CommandState.Description:

                        //    try
                        //    {
                        //        Transaction transactionCache = new();
                        //        var transaction = await _disCache.GetAsync($"{userIdKey}-transaction");
                        //        if (transaction is { Length: > 0 })
                        //        {
                        //            transactionCache = JsonSerializer.Deserialize<Transaction>(transaction);
                        //        }
                        //        transactionCache!.Description = message.Text;
                        //        await CacheExtension.UpdateCacheAsync(_disCache, $"{userIdKey}-transaction", transactionCache);
                        //    }
                        //    catch (JsonException ex)
                        //    {
                        //        await Console.Error.WriteLineAsync($"Failed to deserialize data for key '{userIdKey}': {ex.Message}");
                        //    }

                        //    var inlineKeyboardDescription = new InlineKeyboardMarkup(new[]
                        //     {
                        //        new[]
                        //        {
                        //             InlineKeyboardButton.WithCallbackData("✔️ ثبت نهایی", "AcceptTransaction"),
                        //        },
                        //    });
                        //    await client.SendTextMessageAsync(
                        //   chatId: message.Chat.Id,
                        //   text: "برای ثبت نهایی روی دکمه زیر بزنید \n توضیحات با موفقیت ثبت شد✔️ ",
                        //   replyMarkup: inlineKeyboardDescription);

                        //    session.CommandState = CommandState.AcceptTransaction;
                        //    await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);

                        //    break;
                        //case CommandState.InsertDailyInBound:
                        //    await transactionMenu.SendInBoundTransactionAsync(message.Chat.Id);

                        //    break;
                        //case CommandState.ChooseBank:
                        //    await transactionMenu.SendInBoundTransactionAsync(message.Chat.Id);

                        //    break;
                        //case CommandState.InsertWithDateInBound:
                        //    session.CommandState = CommandState.ChooseBank;
                        //    await CacheExtension.UpdateCacheAsync(_disCache, userIdKey.ToString(), session);
                        //    await transactionMenu.SendInBoundTransactionAsync(message.Chat.Id);

                        //    break;
                        //default:
                        //    break;
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
