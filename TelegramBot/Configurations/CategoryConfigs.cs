using Domain.Entities.Categories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using AutoMapper;
using Telegram.Bot.Types.ReplyMarkups;
using Application.Services.TelegramServices;
using Domain.MapperProfile;

namespace TelegramBot.Configurations
{
    public class CategoryConfigs
    {
        private readonly ITelegramBotClient _client;
        private readonly IDynamicButtonsServices _dynamicButtonsServices;

        public CategoryConfigs(ITelegramBotClient client, IDynamicButtonsServices dynamicButtonsServices)
        {
            _client = client;
            _dynamicButtonsServices = dynamicButtonsServices;
        }

        public async Task SendCategoriesToUser(long chatId)
        {
            await using ApplicationDataContext context = new();
            var cats = await context.Set<Category>().Select(z => new NameValueDTO
            {
                Name = z.Name,
                Id = z.Id
            }).ToListAsync();
            var inlineKeyboards = _dynamicButtonsServices.SetDynamicButtons<NameValueDTO>(4, cats, "category");
            await _client.SendTextMessageAsync(
          chatId: chatId,
          text: $"دسته بندی مورد نظر را انتخاب کنید",
          replyMarkup: inlineKeyboards);
        }
    }
}
