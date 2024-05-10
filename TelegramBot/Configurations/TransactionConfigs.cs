using Domain.Entities.Banks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.BaseMethods;
using TelegramBot.ConstMessages;
using TelegramBot.ConstVariable;

namespace TelegramBot.Configurations
{
    public class TransactionConfigs
    {
        private readonly ITelegramBotClient _client;

        public TransactionConfigs(ITelegramBotClient client)
        {
            _client = client;
        }

        public async Task SendInBoundTransactionAsync(long chatId)
        {
            var inlineKeyboards = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.Today, ConstCallBackData.OutboundTransaction.Daily),
                    InlineKeyboardButton.WithCallbackData(ConstMessage.SpecificDate, ConstCallBackData.OutboundTransaction.SpecificDate),
                },
                new[]
                {
                   InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel)
                }
            });

            await _client.SendTextMessageAsync(
           chatId: chatId,
           text: ConstMessage.OutboundTransactionType,
           parseMode: ParseMode.Html,
           replyMarkup: inlineKeyboards);
        }


        public async Task SendChooseBankAsync(long chatId, long telegramId)
        {
            await using ApplicationDataContext context = new();
            var banks = await context.Set<Bank>().Where(z => z.TelegramId == telegramId).ToListAsync();
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();
            //todo : please create file like app settings json in web api, and handle some config like 4 (column count)
            var columnCount = 4;
            var banksLength = banks.Count / columnCount;
            banksLength = banks.Count % columnCount == 0 ? banksLength : banksLength + 1;
            var index = columnCount;
            var skip = 0;
            for (var i = 0; i < banksLength; i++)
            {
                var tempBanks = banks.Skip(skip).Take(index).ToList();
                var tempKey = tempBanks
                    .Select(bank => InlineKeyboardButton
                        .WithCallbackData(bank.Name, ConstCallBackData.DailyOrSpecificDate.Bank + bank.Id)).ToList();
                inlineKeyboardButtons.Add(tempKey);
                skip = index;
                index = skip;
            }
            inlineKeyboardButtons.Add(new()
            {
                InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back) ,
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,

            });
            var inlineKeyboards = new InlineKeyboardMarkup(inlineKeyboardButtons);
            await _client.SendTextMessageAsync(
          chatId: chatId,
          text: ConstMessage.ChooseBank,
          parseMode: ParseMode.Html,
          replyMarkup: inlineKeyboards);
        }

        public async Task SendPreviewAsync(long chatId, TransactionDto transaction)
        {
            var message = $"{ConstMessage.OutboundTransactionPreview} \nمبلغ: {transaction.Amount}\n" +
                          $"بابت: {transaction.Description}\n" +
                          $"بانک: {transaction.BankId}\n";
            //todo: please get from banks table and replace bank name with bank id,

            var inlineKeyboards = new InlineKeyboardMarkup(new[]
            {
               new[]
               {
                   InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back),
                   InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel),
                   InlineKeyboardButton.WithCallbackData(ConstMessage.Submit, ConstCallBackData.OutboundTransactionPreview.Submit)
               },
            });
            await _client.SendTextMessageAsync(chatId: chatId, text: message, replyMarkup: inlineKeyboards);
        }
    }

}
