using Domain.Entities.Categories;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using static Application.Services.TelegramServices.ConstVariable.ConstCallBackData;
using Application.Services.TelegramServices.ConstVariable;
using Application.Common;
using Application.MapperProfile;

namespace Application.Services.TelegramServices.Configurations
{
    public class CategoryConfigs
    {
        private readonly ITelegramBotClient _client;
        private readonly IDynamicButtonsServices _dynamicButtonsServices;
        private readonly IGenericRepository<Category> _categoryRepository;

        public CategoryConfigs(ITelegramBotClient client, IDynamicButtonsServices dynamicButtonsServices, IGenericRepository<Category> categoryRepository)
        {
            _client = client;
            _dynamicButtonsServices = dynamicButtonsServices;
            _categoryRepository = categoryRepository;
        }

        public async Task SendInBoundCategoriesToUser(long chatId)
        {
            var cats = await _categoryRepository.GetAsNoTrackingQuery().Select(z => new NameValueDTO
            {
                Name = z.Name,
                Id = z.Id
            }).ToListAsync();
            var inlineCategoryKeyboards = _dynamicButtonsServices.SetDynamicButtons(4, cats, InboundDailyCategory.Category);
            inlineCategoryKeyboards.Add(new()
            {
                InlineKeyboardButton.WithCallbackData(ConstMessage.Back, Global.Back) ,
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, OutboundTransactionPreview.Cancel) ,

            });
            var inlineKeyboards = new InlineKeyboardMarkup(inlineCategoryKeyboards);
            await _client.SendTextMessageAsync(
          chatId: chatId,
          text: ConstMessage.ChooseCategory,
          parseMode: ParseMode.Html,
          replyMarkup: inlineKeyboards);
        }



        public async Task SendOutBoundCategoriesToUser(long chatId)
        {
            var cats = await _categoryRepository.GetAsNoTrackingQuery().Select(z => new NameValueDTO
            {
                Name = z.Name,
                Id = z.Id
            }).ToListAsync();

            var inlineCategoryKeyboards = _dynamicButtonsServices.SetDynamicButtons<NameValueDTO>(4, cats, DailyCategory.Category);
            inlineCategoryKeyboards.Add(new()
            {
                InlineKeyboardButton.WithCallbackData(ConstMessage.Back, Global.Back) ,
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, OutboundTransactionPreview.Cancel) ,

            });
            var inlineKeyboards = new InlineKeyboardMarkup(inlineCategoryKeyboards);
            await _client.SendTextMessageAsync(
          chatId: chatId,
          text: ConstMessage.ChooseCategory,
          parseMode: ParseMode.Html,
          replyMarkup: inlineKeyboards);
        }
    }
}
