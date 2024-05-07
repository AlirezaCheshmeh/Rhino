using Domain.Entities.Banks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
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
                    InlineKeyboardButton.WithCallbackData("روزانه", ConstCallBackData.OutboundTransaction.Daily),
                    InlineKeyboardButton.WithCallbackData("تاریخ مشخص", ConstCallBackData.OutboundTransaction.SpecificDate),
                },
            });

            await _client.SendTextMessageAsync(
           chatId: chatId,
           text: $"نوع تراکنش خود را انتخاب کنید",
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
                    .Select(bank => InlineKeyboardButton.WithCallbackData(bank.Name, $"bank-{bank.Id}")).ToList();
                inlineKeyboardButtons.Add(tempKey);
                skip = index;
                index = skip;
            }
            var inlineKeyboards = new InlineKeyboardMarkup(inlineKeyboardButtons);
            await _client.SendTextMessageAsync(
          chatId: chatId,
          text: $"نوع تراکنش خود را انتخاب کنید",
          replyMarkup: inlineKeyboards);
        }
    }

}
