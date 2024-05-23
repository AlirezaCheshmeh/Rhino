using Application.Services.TelegramServices.ConstVariable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Application.Cqrs.Commands;
using Application.Cqrs.Queris;
using Application.Mediator.Transactions.Query;
using Application.Extensions;

namespace Application.Services.TelegramServices.Configurations
{
    public class ReportConfigs
    {

        private readonly ITelegramBotClient _botClient;
        private readonly IQueryDispatcher _queryDispatcher;

        public ReportConfigs(ITelegramBotClient botClient, IQueryDispatcher queryDispatcher)
        {
            _botClient = botClient;
            _queryDispatcher = queryDispatcher;
        }


        public async Task<Message> SendChooseReportType(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.InboundReport, ConstCallBackData.Report.InBound),
                    InlineKeyboardButton.WithCallbackData(ConstMessage.OutBoundReport, ConstCallBackData.Report.OutBound),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                },
            });
            return await _botClient
                .SendTextMessageAsync(chatId, ConstMessage.ChoosereportType, parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard);
        }


        public async Task<Message> SendChooseTypeInboundReport(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.Today, ConstCallBackData.Report.InBoundTodayReport),
                    InlineKeyboardButton.WithCallbackData(ConstMessage.TodaySummary, ConstCallBackData.Report.InBoundTodaySummaryReport),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                },
            });
            return await _botClient
                .SendTextMessageAsync(chatId, ConstMessage.InboundChooseReportType, parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard);
        }


        public async Task<Message> SendChooseTypeOutboundReport(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.Today, ConstCallBackData.Report.OutBoundTodayReport),
                    InlineKeyboardButton.WithCallbackData(ConstMessage.TodaySummary, ConstCallBackData.Report.OutBoundTodaySummaryReport),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                },
            });
            return await _botClient
                .SendTextMessageAsync(chatId, ConstMessage.OutboundChooseReportType, parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard);
        }


        public async Task<Message> SendInboundTodaySummary(long chatId, long telegramId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu),
                },
            });
            var summary = await _queryDispatcher.SendAsync(new GetTodaySummeryQuery { TelegramId = telegramId });
            var data = summary.Data;
            return await _botClient.SendTextMessageAsync(
            chatId,
            $"<b>خلاصه دریافتی شما امروز در تاریخ:</b> <b>{DateExtension.ConvertToPersianDate(DateTime.Now.ToString("yyy/MM/dd")).ToPersianNumber()} \n به شرح زیر می‌باشد ⬇️</b> \n" +
            $"<b>💰 مجموع دریافتی:</b> <b>{data.SumAmount} تومان</b>\n" +
            $"<b>📈 بیشترین مبلغ دریافتی امروز:</b> <b>{data.BiggestOutBound} تومان </b>\n" +
            $"<b>📝 بابت بیشترین دریافتی:</b> <b> {(string.IsNullOrEmpty(data.Description) ? "موردی یافت نشد" : data.Description)} </b>\n" +
            $"<b>🏦 بانک بیشترین دریافتی:</b> <b> {data.BankTransaction}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard
            );
        }

        public async Task<Message> SendOutboundTodaySummary(long chatId, long telegramId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu),
                },
            });
            var summary = await _queryDispatcher.SendAsync(new GetOutBoundTransactionTodaySummary { TelegramId = telegramId });
            var data = summary.Data;
            return await _botClient.SendTextMessageAsync(
            chatId,
            $"<b>خلاصه پرداختی شما امروز در تاریخ:</b> <b>{DateExtension.ConvertToPersianDate(DateTime.Now.ToString("yyy/MM/dd")).ToPersianNumber()} \n به شرح زیر می‌باشد ⬇️</b> \n" +
            $"<b>💸 مجموع پرداختی:</b> <b>{data.SumAmount} تومان</b> \n" +
            $"<b>📉 بیشترین مبلغ پرداختی امروز:</b>  <b>{data.BiggestOutBound} تومان </b>\n" +
            $"<b>📝 بابت بیشترین پرداختی:</b> <b>{(string.IsNullOrEmpty(data.Description) ? "موردی یافت نشد" : data.Description)} </b> \n" +
            $"<b>🏦 بانک بیشترین پرداختی:</b> <b>{data.BankTransaction}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard);
        }
    }
}
