using Application.Extensions;
using Application.Services.TelegramServices;
using Domain.Entities.Banks;
using Domain.Entities.Categories;
using Domain.MapperProfile;
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
        private readonly IDynamicButtonsServices _dynamicButtons;

        public TransactionConfigs(ITelegramBotClient client, IDynamicButtonsServices dynamicButtons)
        {
            _client = client;
            _dynamicButtons = dynamicButtons;
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
            var banks = await context.Set<Bank>().Where(z => z.TelegramId == telegramId).Select(x=> new NameValueDTO
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            var bankInlineKeyboardButtons = _dynamicButtons.SetDynamicButtons(4, banks, ConstCallBackData.DailyOrSpecificDate.Bank);
            bankInlineKeyboardButtons.Add(new()
            {
                InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back) ,
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,

            });
            var inlineKeyboards = new InlineKeyboardMarkup(bankInlineKeyboardButtons);
            await _client.SendTextMessageAsync(
          chatId: chatId,
          text: ConstMessage.ChooseBank,
          parseMode: ParseMode.Html,
          replyMarkup: inlineKeyboards);
        }

        public async Task SendPreviewAsync(long chatId, TransactionDto transaction)
        {
            using Infrastructure.Database.ApplicationDataContext context = new();
            var bankName = (await context.Set<Bank>().Where(z => z.Id == transaction.BankId.Value).FirstOrDefaultAsync()).Name;
            var CatName = (await context.Set<Category>().Where(z => z.Id == transaction.CategoryId.Value).FirstOrDefaultAsync()).Name;
            var message = $"{ConstMessage.OutboundTransactionPreview} \nمبلغ: {NumbersConvertorExtension.ToPersianNumber(transaction.Amount.Value.ToString("N0"))}\n" +
                          $"بابت: {transaction.Description}\n" +
                          $"دسته یندی: {CatName}\n" +
                          $"بانک: {bankName}\n";
            //todo: please get from banks table and replace bank name with bank id,

            var inlineKeyboards = new InlineKeyboardMarkup(new[]
            {
               new[]
               {
                   InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel),
                   InlineKeyboardButton.WithCallbackData(ConstMessage.Submit, ConstCallBackData.OutboundTransactionPreview.Submit)
               },
            });
            await _client.SendTextMessageAsync(chatId: chatId, text: message, replyMarkup: inlineKeyboards);
        }
    }

}
