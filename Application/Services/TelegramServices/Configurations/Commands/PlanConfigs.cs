using Application.Common;
using Application.Extensions;
using Application.Services.TelegramServices.ConstVariable;
using Domain.Entities.Plans;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Application.Services.TelegramServices.Configurations.Commands
{
    public class PlanConfigs
    {

        private readonly ITelegramBotClient _client;
        private readonly IGenericRepository<Plan> _planRepository;

        public PlanConfigs(ITelegramBotClient client, IGenericRepository<Plan> planRepository)
        {
            _client = client;
            _planRepository = planRepository;
        }

        public async Task SendPlanToUser(long chatId)
        {
            var plans = await _planRepository.GetAsNoTrackingQuery().ToListAsync();

            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();

            foreach (var plan in plans)
            {
                var inlineButtons = new List<InlineKeyboardButton>();

                var amountText = NumbersConvertorExtension.ToPersianNumber(plan.Price.ToString("N0"));
                var priceButton = InlineKeyboardButton.WithCallbackData($"{amountText} تومان", $"#");
                inlineButtons.Add(priceButton);

                var titleButton = InlineKeyboardButton.WithCallbackData(plan.Title, $"#");
                inlineButtons.Add(titleButton);

                inlineKeyboardButtons.Add(inlineButtons);
            }
            inlineKeyboardButtons.Add(new() { InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu) });
            InlineKeyboardMarkup markup = new(inlineKeyboardButtons);

            await _client.SendTextMessageAsync(
                chatId: chatId,
                text: ConstMessage.BuyAccount,
                parseMode: ParseMode.Html,
                replyMarkup: markup);
        }
    }
}
