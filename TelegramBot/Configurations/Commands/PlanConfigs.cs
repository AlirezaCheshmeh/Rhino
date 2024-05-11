using Application.Extensions;
using Domain.Entities.Plans;
using Domain.MapperProfile;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.ConstMessages;
using TelegramBot.ConstVariable;

namespace TelegramBot.Configurations.Commands
{
    public class PlanConfigs
    {

        private readonly ITelegramBotClient _client;

        public PlanConfigs(ITelegramBotClient client)
        {
            _client = client;
        }

        public async Task SendPlanToUser(long chatId)
        {
            using ApplicationDataContext context = new();
            var plans = await context.Set<Plan>().ToListAsync();

            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();

            foreach (var plan in plans)
            {
                var inlineButtons = new List<InlineKeyboardButton>();

                var amountText = NumbersConvertorExtension.ToPersianNumber(plan.Price.ToString("N0"));
                var priceButton = InlineKeyboardButton.WithCallbackData($"{amountText} تومان" , $"#");
                inlineButtons.Add(priceButton);

                var titleButton = InlineKeyboardButton.WithCallbackData(plan.Title, $"#");
                inlineButtons.Add(titleButton);

                inlineKeyboardButtons.Add(inlineButtons);
            }
            inlineKeyboardButtons.Add(new(){InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu)});
            InlineKeyboardMarkup markup = new(inlineKeyboardButtons);

            await _client.SendTextMessageAsync(
                chatId: chatId,
                text: ConstMessage.BuyAccount,
                parseMode: ParseMode.Html,
                replyMarkup: markup);
        }
    }
}
