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

namespace TelegramBot.Configurations
{
    public class TransactionTelegramConfigs
    {
        private readonly ITelegramBotClient _client;

        public TransactionTelegramConfigs(ITelegramBotClient client)
        {
            _client = client;
        }

        public async Task SendInBoundTransactionAsync(long chatId)
        {
            var inlineKeyboards = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("☀️ روزانه", "InsertDailyTransaction"),
                    InlineKeyboardButton.WithCallbackData("🗓 تاریخ مشخص", "InsertWithDateTransaction"),
                },
            });

            await _client.SendTextMessageAsync(
           chatId: chatId,
           text: $"نوع تراکنش خود را انتخاب کنید",
           replyMarkup: inlineKeyboards);
        }


        public async Task SendChooseBankAsync(long chatId, long telegramId)
        {
   
            using ApplicationDataContext context = new();

            var banks = await context.Set<Bank>()
                .Where(z => z.TelegramId == telegramId)
                .ToListAsync();
            var inlineKeyboardRows = new List<InlineKeyboardButton[]>();
            var currentRow = new List<InlineKeyboardButton>();
            foreach (var bank in banks)
            {
                var button = InlineKeyboardButton.WithCallbackData(bank.Name, $"bank-{bank.Id}");
                currentRow.Add(button);

                if (currentRow.Count == 4)
                {
                    inlineKeyboardRows.Add(currentRow.ToArray());
                    currentRow.Clear();
                }
            }
            if (currentRow.Count > 0)
            {
                inlineKeyboardRows.Add(currentRow.ToArray());
            }
            var inlineKeyboards = new InlineKeyboardMarkup(inlineKeyboardRows);
            await _client.SendTextMessageAsync(
                chatId: chatId,
                text: "نوع تراکنش خود را انتخاب کنید",
                replyMarkup: inlineKeyboards);
        }
    }

}
