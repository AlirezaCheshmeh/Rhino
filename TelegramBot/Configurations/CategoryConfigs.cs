﻿using Domain.Entities.Categories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using AutoMapper;
using Telegram.Bot.Types.ReplyMarkups;
using Application.Services.TelegramServices;
using Application.MapperProfile;
using Telegram.Bot.Types.Enums;
using TelegramBot.ConstMessages;
using TelegramBot.ConstVariable;
using static TelegramBot.ConstVariable.ConstCallBackData;

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

        //public async Task SendInBoundCategoriesToUser(long chatId)
        //{
        //    await using ApplicationDataContext context = new();
        //    var cats = await context.Set<Category>().Select(z => new NameValueDTO
        //    {
        //        Name = z.Name,
        //        Id = z.Id
        //    }).ToListAsync();
        //    var inlineCategoryKeyboards = _dynamicButtonsServices.SetDynamicButtons<NameValueDTO>(4, cats, InboundDailyCategory.Category);
        //    inlineCategoryKeyboards.Add(new()
        //    {
        //        InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back) ,
        //        InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,

        //    });
        //    var inlineKeyboards = new InlineKeyboardMarkup(inlineCategoryKeyboards);
        //    await _client.SendTextMessageAsync(
        //  chatId: chatId,
        //  text: ConstMessage.ChooseCategory,
        //  parseMode:ParseMode.Html,
        //  replyMarkup: inlineKeyboards);
        //}



        //public async Task SendOutBoundCategoriesToUser(long chatId)
        //{
        //    await using ApplicationDataContext context = new();
        //    var cats = await context.Set<Category>().Select(z => new NameValueDTO
        //    {
        //        Name = z.Name,
        //        Id = z.Id
        //    }).ToListAsync();
        //    var inlineCategoryKeyboards = _dynamicButtonsServices.SetDynamicButtons<NameValueDTO>(4, cats, DailyCategory.Category);
        //    inlineCategoryKeyboards.Add(new()
        //    {
        //        InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back) ,
        //        InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,

        //    });
        //    var inlineKeyboards = new InlineKeyboardMarkup(inlineCategoryKeyboards);
        //    await _client.SendTextMessageAsync(
        //  chatId: chatId,
        //  text: ConstMessage.ChooseCategory,
        //  parseMode: ParseMode.Html,
        //  replyMarkup: inlineKeyboards);
        //}
    }
}
