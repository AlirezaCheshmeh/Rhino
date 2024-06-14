using Application.Common;
using Application.Extensions;
using Application.MapperProfile;
using Application.Services.TelegramServices.BaseMethods;
using Application.Services.TelegramServices.ConstVariable;
using Domain.Entities.Banks;
using Domain.Entities.Categories;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Application.Services.TelegramServices.Configurations
{
    public class TransactionConfigs
    {
        private readonly ITelegramBotClient _client;
        private readonly IDynamicButtonsServices _dynamicButtons;
        private readonly IGenericRepository<Bank> _bankRepository;
        private readonly IGenericRepository<Category> _categoryRepository;

        public TransactionConfigs(ITelegramBotClient client, IDynamicButtonsServices dynamicButtons, IGenericRepository<Bank> bankRepository, IGenericRepository<Category> categoryRepository)
        {
            _client = client;
            _dynamicButtons = dynamicButtons;
            _bankRepository = bankRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task SendOutBoundTransactionAsync(long chatId)
        {
            var inlineKeyboards = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.Today, ConstCallBackData.OutboundTransaction.Daily),
                    InlineKeyboardButton.WithCallbackData(ConstMessage.SpecificDate, ConstCallBackData.OutboundTransaction.OutBoundSpecificDate),
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


        public async Task SendInBoundTransactionAsync(long chatId)
        {
            var inlineKeyboards = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.Today, ConstCallBackData.InboundTransaction.Daily),
                    InlineKeyboardButton.WithCallbackData(ConstMessage.SpecificDate, ConstCallBackData.InboundTransaction.InBoundSpecificDate),
                },
                new[]
                {
                   InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.InboundTransactionPreview.Cancel)
                }
            });

            await _client.SendTextMessageAsync(
           chatId: chatId,
           text: ConstMessage.IntboundTransactionType,
           parseMode: ParseMode.Html,
           replyMarkup: inlineKeyboards);
        }


        public async Task SendChooseBankAsync(long chatId, long telegramId)
        {

            var banks = await _bankRepository.GetAsNoTrackingQuery().Where(z => z.TelegramId == telegramId).Select(x => new NameValueDTO
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

        public async Task SendInBoundChooseBankAsync(long chatId, long telegramId)
        {
            var banks = await _bankRepository.GetAsNoTrackingQuery().Where(z => z.TelegramId == telegramId).Select(x => new NameValueDTO
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();


            var bankInlineKeyboardButtons = _dynamicButtons.SetDynamicButtons(4, banks, ConstCallBackData.InboundDailyOrSpecificDate.Bank);
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
            var bankName = (await _bankRepository.GetAsNoTrackingQuery().Where(z => z.Id == transaction.BankId.Value).FirstOrDefaultAsync()).Name;
            var CatName = (await _categoryRepository.GetAsNoTrackingQuery().Where(z => z.Id == transaction.CategoryId.Value).FirstOrDefaultAsync()).Name;
            var message = new StringBuilder($"{ConstMessage.OutboundTransactionPreview} \n مبلغ: {transaction.Amount.Value.ToString("N0").ToPersianNumber()}\n" + 
                          $"بابت: {transaction.Description}\n" +
                          $"دسته بندی: {CatName}\n" +
                          $"بانک: {bankName}\n" +
                          $"نوع: {transaction.Type.ToDisplay()}\n");
            if (transaction.CreatedAt.HasValue)
                message.Append($"در تاریخ: {DateExtension.ConvertToPersianDate(transaction.CreatedAt.Value.ToString("yyy/MM/dd")).ToPersianNumber()}");

            var inlineKeyboards = new InlineKeyboardMarkup(new[]
            {
               new[]
               {
                   InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel),
                   InlineKeyboardButton.WithCallbackData(ConstMessage.Submit,transaction.Type == Domain.Enums.TransactionType.OutBound?ConstCallBackData.OutboundTransactionPreview.Submit:ConstCallBackData.InboundTransactionPreview.Submit)
               },
            });
            await _client.SendTextMessageAsync(chatId: chatId, text: message.ToString(), replyMarkup: inlineKeyboards);
        }


       
    }

}
