using Application.Common;
using Domain.Entities.Banks;
using Domain.Entities.Categories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Configurations
{
    public class CategoryConfigs
    {
        private readonly ITelegramBotClient _client;

        public CategoryConfigs(ITelegramBotClient client)
        {
            _client = client;
        }

        public async Task SendCategoriesToUser(long chatId)
        {
            const int rows = 4;
            await using ApplicationDataContext context = new();
            var cats = await context.Set<Category>().Select(z => new
            {
                Name = z.Name,
                Id = z.Id
            }).ToListAsync();
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();
            //todo : please create file like app settings json in web api, and handle some config like 4 (column count)
            var columnCount = 4;
            var banksLength = cats.Count / columnCount;
            banksLength = cats.Count % columnCount == 0 ? banksLength : banksLength + 1;
            var index = columnCount;
            var skip = 0;
            for (var i = 0; i < banksLength; i++)
            {
                var tempBanks = cats.Skip(skip).Take(index).ToList();
                var tempKey = tempBanks
                    .Select(cat => InlineKeyboardButton.WithCallbackData(cat.Name, $"category-{cat.Id}")).ToList();
                inlineKeyboardButtons.Add(tempKey);
                skip = index;
                index = skip;
            }
            var inlineKeyboards = new InlineKeyboardMarkup(inlineKeyboardButtons);
            await _client.SendTextMessageAsync(
          chatId: chatId,
          text: $"دسته بندی مورد نظر را انتخاب کنید",
          replyMarkup: inlineKeyboards);
        }
    }
}
