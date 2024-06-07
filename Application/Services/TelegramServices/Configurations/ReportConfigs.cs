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
using static Application.Services.TelegramServices.BaseMethods.HandleUpdate;
using Microsoft.Extensions.Caching.Distributed;
using Domain.Entities.Transactions;
using Application.Mediator.Transactions.DTOs;
using Application.Common;
using Domain.Entities.Banks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Categories;

namespace Application.Services.TelegramServices.Configurations
{
    public class ReportConfigs
    {

        private readonly ITelegramBotClient _botClient;
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly IDistributedCache _distributedCache;
        private readonly IGenericRepository<Bank> _bankRepo;
        private readonly IGenericRepository<Category> _catRepo;

        public ReportConfigs(ITelegramBotClient botClient, IQueryDispatcher queryDispatcher, IDistributedCache distributedCache, IGenericRepository<Bank> bankRepo, IGenericRepository<Category> catRepo)
        {
            _botClient = botClient;
            _queryDispatcher = queryDispatcher;
            _distributedCache = distributedCache;
            _bankRepo = bankRepo;
            _catRepo = catRepo;
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


        public async Task<Message> SendChooseTimeTypeOutboundReport(long chatId, int MessageId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.Today, ConstCallBackData.Report.OutboundToday),
                    InlineKeyboardButton.WithCallbackData(ConstMessage.Yesterday, ConstCallBackData.Report.OutboundYesterday),
                    InlineKeyboardButton.WithCallbackData(ConstMessage.LastWeek, ConstCallBackData.Report.OutboundLastWeek),
                    InlineKeyboardButton.WithCallbackData(ConstMessage.LastMonth, ConstCallBackData.Report.OutboundLastMonth),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                },
            });
            return await _botClient
                .EditMessageTextAsync(chatId, MessageId, ConstMessage.OutboundChooseReportTimeType, parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard);
        }

        public async Task<Message> SendChooseTimeTypeInboundReport(long chatId, int MessageId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.Today, ConstCallBackData.Report.InboundToday),
                    InlineKeyboardButton.WithCallbackData(ConstMessage.Yesterday, ConstCallBackData.Report.InboundYesterday),
                    InlineKeyboardButton.WithCallbackData(ConstMessage.LastWeek, ConstCallBackData.Report.InboundLastWeek),
                    InlineKeyboardButton.WithCallbackData(ConstMessage.LastMonth, ConstCallBackData.Report.InboundLastMonth),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                },
            });
            return await _botClient
                .EditMessageTextAsync(chatId, MessageId, ConstMessage.InboundChooseReportTimeType, parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard);
        }
        //main
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


        public async Task<Message> SendInboundTodaySummary(long chatId, int MessageId, long telegramId)
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
            return await _botClient.EditMessageTextAsync(
            chatId, MessageId,
            $"<b>خلاصه دریافتی شما امروز در تاریخ:</b> <b>{DateExtension.ConvertToPersianDate(DateTime.Now.ToString("yyy/MM/dd")).ToPersianNumber()} \n به شرح زیر می‌باشد ⬇️</b> \n" +
            $"<b>💰 مجموع دریافتی:</b> <b>{data.SumAmount} تومان</b>\n" +
            $"<b>📈 بیشترین مبلغ دریافتی امروز:</b> <b>{data.BiggestOutBound} تومان </b>\n" +
            $"<b>📝 بابت بیشترین دریافتی:</b> <b> {(string.IsNullOrEmpty(data.Description) ? "موردی یافت نشد" : data.Description)} </b>\n" +
            $"<b>🏦 بانک بیشترین دریافتی:</b> <b> {data.BankTransaction}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard
            );
        }

        public async Task<Message> SendOutboundTodaySummary(long chatId, int MessageId, long telegramId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu),
                },
            });
            var summary = await _queryDispatcher.SendAsync(new GetOutBoundTransactionTodaySummary { TelegramId = telegramId, FromDate = DateTime.Now, ToDate = DateTime.Now });
            var data = summary.Data;
            return await _botClient.EditMessageTextAsync(
            chatId, MessageId,
            $"<b>خلاصه پرداختی شما امروز در تاریخ:</b> <b>{DateExtension.ConvertToPersianDate(DateTime.Now.ToString("yyy/MM/dd")).ToPersianNumber()} \n به شرح زیر می‌باشد ⬇️</b> \n" +
            $"<b>💸 مجموع پرداختی:</b> <b>{data.SumAmount} تومان</b> \n" +
            $"<b>📉 بیشترین مبلغ پرداختی امروز:</b>  <b>{data.BiggestOutBound} تومان </b>\n" +
            $"<b>📝 بابت بیشترین پرداختی:</b> <b>{(string.IsNullOrEmpty(data.Description) ? "موردی یافت نشد" : data.Description)} </b> \n" +
            $"<b>🏦 بانک بیشترین پرداختی:</b> <b>{data.BankTransaction}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard);
        }

        public async Task<Message> SendOutboundYesterdaySummary(long chatId, int MessageId, long telegramId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu),
                },
            });
            var summary = await _queryDispatcher.SendAsync(new GetOutBoundTransactionTodaySummary { TelegramId = telegramId, FromDate = DateTime.Now.AddDays(-1), ToDate = DateTime.Now.AddDays(-1) });
            var data = summary.Data;
            return await _botClient.EditMessageTextAsync(
            chatId, MessageId,
            $"<b>خلاصه پرداختی شما دیروز در تاریخ:</b> <b>{DateExtension.ConvertToPersianDate(DateTime.Now.AddDays(-1).ToString("yyy/MM/dd")).ToPersianNumber()} \n به شرح زیر می‌باشد ⬇️</b> \n" +
            $"<b>💸 مجموع پرداختی:</b> <b>{data.SumAmount} تومان</b> \n" +
            $"<b>📉 بیشترین مبلغ پرداختی دیروز:</b>  <b>{data.BiggestOutBound} تومان </b>\n" +
            $"<b>📝 بابت بیشترین پرداختی:</b> <b>{(string.IsNullOrEmpty(data.Description) ? "موردی یافت نشد" : data.Description)} </b> \n" +
            $"<b>🏦 بانک بیشترین پرداختی:</b> <b>{data.BankTransaction}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard);
        }

        public async Task<Message> SendOutboundlastWeekSummary(long chatId, int MessageId, long telegramId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu),
                },
            });
            var summary = await _queryDispatcher.SendAsync(new GetOutBoundTransactionTodaySummary { TelegramId = telegramId, FromDate = DateTime.Now.AddDays(-7),Type = Domain.Enums.TransactionType.OutBound });
            var data = summary.Data;
            return await _botClient.EditMessageTextAsync(
            chatId, MessageId,
            $"<b>خلاصه پرداختی شما در هفته اخیر \n به شرح زیر می‌باشد ⬇️</b> \n" +
            $"<b>💸 مجموع پرداختی:</b> <b>{data.SumAmount} تومان</b> \n" +
            $"<b>📉 بیشترین مبلغ پرداختی در هفته اخیر:</b>  <b>{data.BiggestOutBound} تومان </b>\n" +
            $"<b>📝 بابت بیشترین پرداختی:</b> <b>{(string.IsNullOrEmpty(data.Description) ? "موردی یافت نشد" : data.Description)} </b> \n" +
            $"<b>🏦 بانک بیشترین پرداختی:</b> <b>{data.BankTransaction}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard);
        }

        public async Task<Message> SendOutboundlastMonthSummary(long chatId, int MessageId, long telegramId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu),
                },
            });
            var summary = await _queryDispatcher.SendAsync(new GetOutBoundTransactionTodaySummary { TelegramId = telegramId, FromDate = DateTime.Now.AddDays(-30), Type = Domain.Enums.TransactionType.OutBound });
            var data = summary.Data;
            return await _botClient.EditMessageTextAsync(
            chatId, MessageId,
            $"<b>خلاصه پرداختی شما در ماه اخیر \n به شرح زیر می‌باشد ⬇️</b> \n" +
            $"<b>💸 مجموع پرداختی:</b> <b>{data.SumAmount} تومان</b> \n" +
            $"<b>📉 بیشترین مبلغ پرداختی در ماه اخیر:</b>  <b>{data.BiggestOutBound} تومان </b>\n" +
            $"<b>📝 بابت بیشترین پرداختی:</b> <b>{(string.IsNullOrEmpty(data.Description) ? "موردی یافت نشد" : data.Description)} </b> \n" +
            $"<b>🏦 بانک بیشترین پرداختی:</b> <b>{data.BankTransaction}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard);
        }

        public async Task<Message> SendInboundlastWeekSummary(long chatId, int MessageId, long telegramId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu),
                },
            });
            var summary = await _queryDispatcher.SendAsync(new GetOutBoundTransactionTodaySummary { TelegramId = telegramId, FromDate = DateTime.Now.AddDays(-7),Type = Domain.Enums.TransactionType.InBound });
            var data = summary.Data;
            return await _botClient.EditMessageTextAsync(
            chatId, MessageId,
            $"<b>خلاصه دریافتی شما در هفته اخیر \n به شرح زیر می‌باشد ⬇️</b> \n" +
            $"<b>💸 مجموع دریافتی:</b> <b>{data.SumAmount} تومان</b> \n" +
            $"<b>📉 بیشترین مبلغ دریافتی در هفته اخیر:</b>  <b>{data.BiggestOutBound} تومان </b>\n" +
            $"<b>📝 بابت بیشترین دریافتی:</b> <b>{(string.IsNullOrEmpty(data.Description) ? "موردی یافت نشد" : data.Description)} </b> \n" +
            $"<b>🏦 بانک بیشترین دریافتی:</b> <b>{data.BankTransaction}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard);
        }

        public async Task<Message> SendInboundlastMonthSummary(long chatId, int MessageId, long telegramId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu),
                },
            });
            var summary = await _queryDispatcher.SendAsync(new GetOutBoundTransactionTodaySummary { TelegramId = telegramId, FromDate = DateTime.Now.AddDays(-30), Type = Domain.Enums.TransactionType.InBound });
            var data = summary.Data;
            return await _botClient.EditMessageTextAsync(
            chatId, MessageId,
            $"<b>خلاصه دریافتی شما در ماه اخیر \n به شرح زیر می‌باشد ⬇️</b> \n" +
            $"<b>💸 مجموع دریافتی:</b> <b>{data.SumAmount} تومان</b> \n" +
            $"<b>📉 بیشترین مبلغ دریافتی در ماه اخیر:</b>  <b>{data.BiggestOutBound} تومان </b>\n" +
            $"<b>📝 بابت بیشترین دریافتی:</b> <b>{(string.IsNullOrEmpty(data.Description) ? "موردی یافت نشد" : data.Description)} </b> \n" +
            $"<b>🏦 بانک بیشترین دریافتی:</b> <b>{data.BankTransaction}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard);
        }

        public async Task<Message> SendInboundYesterdaySummary(long chatId, int MessageId, long telegramId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu),
                },
            });
            var summary = await _queryDispatcher.SendAsync(new GetOutBoundTransactionTodaySummary {Type = Domain.Enums.TransactionType.InBound, TelegramId = telegramId, FromDate = DateTime.Now.AddDays(-1), ToDate = DateTime.Now.AddDays(-1) });
            var data = summary.Data;
            return await _botClient.EditMessageTextAsync(
            chatId, MessageId,
            $"<b>خلاصه دریافتی شما دیروز در تاریخ:</b> <b>{DateExtension.ConvertToPersianDate(DateTime.Now.AddDays(-1).ToString("yyy/MM/dd")).ToPersianNumber()} \n به شرح زیر می‌باشد ⬇️</b> \n" +
            $"<b>💸 مجموع دریافتی:</b> <b>{data.SumAmount} تومان</b> \n" +
            $"<b>📉 بیشترین مبلغ دریافتی دیروز:</b>  <b>{data.BiggestOutBound} تومان </b>\n" +
            $"<b>📝 بابت بیشترین دریافتی:</b> <b>{(string.IsNullOrEmpty(data.Description) ? "موردی یافت نشد" : data.Description)} </b> \n" +
            $"<b>🏦 بانک بیشترین دریافتی:</b> <b>{data.BankTransaction}</b>",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard);
        }
        public async Task SendOutboundToday(long chatId, int MessageId, long telegramId, UserSession session)
        {
            var menu = new MenuConfigs(_botClient, _distributedCache);
            var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = 1, FromDate = DateTime.Now.Date, ToDate = DateTime.Now.Date, TelegramId = telegramId, Type = Domain.Enums.TransactionType.OutBound });
            if (data.Data.Count is 0)
            {
                var message = await _botClient.EditMessageTextAsync(chatId, MessageId, "<b>تراکنشی پرداختی برای امروز وجود ندارد</b> ⚠", parseMode: ParseMode.Html);
                session.MessageIds.Add(message.MessageId);
                await Task.Delay(300);
                await menu.RollBackToMenu(telegramId, chatId, session);
            }
            else
            {
                var transactions = data.Data;
                var inlineKeyboard = await GenerateOutBoundPagination(transactions, 1, data.TotalCount.Value);
                await _botClient.EditMessageTextAsync(chatId, MessageId, $"💸 <b>تراکنش‌های پرداختی امروز</b> \n 📉 <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()} \n 📝 شماره صفحه:{1.ToString().ToPersianNumber()} </b> \n💰 <b>مجموع:{data.TotalAmount} تومان</b>", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineKeyboard);
            }
        }


        public async Task SendInboundToday(long chatId, long telegramId, int MessageId, UserSession session)
        {
            var menu = new MenuConfigs(_botClient, _distributedCache);
            var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = 1, FromDate = DateTime.Now.Date, ToDate = DateTime.Now.Date, TelegramId = telegramId, Type = Domain.Enums.TransactionType.InBound });
            if (data.Data.Count is 0)
            {
                var message = await _botClient.SendTextMessageAsync(chatId, "<b>تراکنشی دریافتی برای امروز وجود ندارد</b> ⚠", parseMode: ParseMode.Html);
                session.MessageIds.Add(message.MessageId);
                await Task.Delay(300);
                await menu.RollBackToMenu(telegramId, chatId, session);
            }
            else
            {
                var transactions = data.Data;
                var inlineKeyboard = await GenerateInBoundPagination(transactions, 1, data.TotalCount.Value);
                await _botClient.EditMessageTextAsync(chatId, MessageId, $"💸 <b>تراکنش‌های دریافتی امروز</b>  \n📉  <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}  \n📝 شماره صفحه:{1.ToString().ToPersianNumber()} </b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b> ", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineKeyboard);
            }
        }

        public async Task SendInboundLastMonth(long chatId, long telegramId, int MessageId, UserSession session)
        {
            var menu = new MenuConfigs(_botClient, _distributedCache);
            var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = 1, FromDate = DateTime.Now.AddDays(-30).Date, TelegramId = telegramId, Type = Domain.Enums.TransactionType.InBound });
            if (data.Data.Count is 0)
            {
                var message = await _botClient.SendTextMessageAsync(chatId, "<b>تراکنشی دریافتی در ماه اخیر وجود ندارد</b> ⚠", parseMode: ParseMode.Html);
                session.MessageIds.Add(message.MessageId);
                await Task.Delay(300);
                await menu.RollBackToMenu(telegramId, chatId, session);
            }
            else
            {
                var transactions = data.Data;
                var inlineKeyboard = await GenerateInBoundLastMonthPagination(transactions, 1, data.TotalCount.Value);
                await _botClient.EditMessageTextAsync(chatId, MessageId, $"💸 <b>تراکنش‌های دریافتی در ماه اخیر</b>  \n📉  <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}  \n📝 شماره صفحه:{1.ToString().ToPersianNumber()} </b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b> ", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineKeyboard);
            }
        }

        public async Task SendOutboundLastMonth(long chatId, long telegramId, int MessageId, UserSession session)
        {
            var menu = new MenuConfigs(_botClient, _distributedCache);
            var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = 1, FromDate = DateTime.Now.AddDays(-30).Date, TelegramId = telegramId, Type = Domain.Enums.TransactionType.OutBound });
            if (data.Data.Count is 0)
            {
                var message = await _botClient.SendTextMessageAsync(chatId, "<b>تراکنشی پرداختی در ماه اخیر وجود ندارد</b> ⚠", parseMode: ParseMode.Html);
                session.MessageIds.Add(message.MessageId);
                await Task.Delay(300);
                await menu.RollBackToMenu(telegramId, chatId, session);
            }
            else
            {
                var transactions = data.Data;
                var inlineKeyboard = await GenerateOutBoundLastMonthPagination(transactions, 1, data.TotalCount.Value);
                await _botClient.EditMessageTextAsync(chatId, MessageId, $"💸 <b>تراکنش‌های پرداختی در ماه اخیر</b>  \n📉  <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}  \n📝 شماره صفحه:{1.ToString().ToPersianNumber()} </b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b> ", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineKeyboard);
            }
        }

        public async Task SendInboundLastWeek(long chatId, long telegramId, int MessageId, UserSession session)
        {
            var menu = new MenuConfigs(_botClient, _distributedCache);
            var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = 1, FromDate = DateTime.Now.AddDays(-7).Date,TelegramId = telegramId, Type = Domain.Enums.TransactionType.InBound });
            if (data.Data.Count is 0)
            {
                var message = await _botClient.SendTextMessageAsync(chatId, "<b>تراکنشی دریافتی در هفته اخیر وجود ندارد</b> ⚠", parseMode: ParseMode.Html);
                session.MessageIds.Add(message.MessageId);
                await Task.Delay(300);
                await menu.RollBackToMenu(telegramId, chatId, session);
            }
            else
            {
                var transactions = data.Data;
                var inlineKeyboard = await GenerateInBoundLastWeekPagination(transactions, 1, data.TotalCount.Value);
                await _botClient.EditMessageTextAsync(chatId, MessageId, $"💸 <b>تراکنش‌های دریافتی در هفته اخیر</b>  \n📉  <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}  \n📝 شماره صفحه:{1.ToString().ToPersianNumber()} </b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b> ", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineKeyboard);
            }
        }

        public async Task SendOutboundLastWeek(long chatId, long telegramId, int MessageId, UserSession session)
        {
            var menu = new MenuConfigs(_botClient, _distributedCache);
            var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = 1, FromDate = DateTime.Now.AddDays(-7).Date, TelegramId = telegramId, Type = Domain.Enums.TransactionType.OutBound });
            if (data.Data.Count is 0)
            {
                var message = await _botClient.SendTextMessageAsync(chatId, "<b>تراکنشی پرداختی در هفته اخیر وجود ندارد</b> ⚠", parseMode: ParseMode.Html);
                session.MessageIds.Add(message.MessageId);
                await Task.Delay(300);
                await menu.RollBackToMenu(telegramId, chatId, session);
            }
            else
            {
                var transactions = data.Data;
                var inlineKeyboard = await GenerateOutBoundLastWeekPagination(transactions, 1, data.TotalCount.Value);
                await _botClient.EditMessageTextAsync(chatId, MessageId, $"💸 <b>تراکنش‌های پرداختی در هفته اخیر</b>  \n📉  <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}  \n📝 شماره صفحه:{1.ToString().ToPersianNumber()} </b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b> ", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineKeyboard);
            }
        }

        public async Task SendInboundYesterday(long chatId, long telegramId, int MessageId, UserSession session)
        {
            var menu = new MenuConfigs(_botClient, _distributedCache);
            var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = 1, FromDate = DateTime.Now.AddDays(-1).Date, ToDate = DateTime.Now.AddDays(-1).Date, TelegramId = telegramId, Type = Domain.Enums.TransactionType.InBound });
            if (data.Data.Count is 0)
            {
                var message = await _botClient.SendTextMessageAsync(chatId, "<b>تراکنشی دریافتی برای دیروز وجود ندارد</b> ⚠", parseMode: ParseMode.Html);
                session.MessageIds.Add(message.MessageId);
                await Task.Delay(300);
                await menu.RollBackToMenu(telegramId, chatId, session);
            }
            else
            {
                var transactions = data.Data;
                var inlineKeyboard = await GenerateInBoundYesterdayPagination(transactions, 1, data.TotalCount.Value);
                await _botClient.EditMessageTextAsync(chatId, MessageId, $"💸 <b>تراکنش‌های دریافتی دیروز</b>  \n📉  <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}  \n📝 شماره صفحه:{1.ToString().ToPersianNumber()} </b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b> ", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineKeyboard);
            }
        }


        // today outbound
        public async Task<InlineKeyboardMarkup> GenerateOutBoundPagination(List<TransactionDTO> transactions, int currentPage, int totalCount)
        {
            int rowsPerPage = 5;
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();


            int counter = 0;
            foreach (var transaction in transactions)
            {
                counter++;
                var bankName = (await _bankRepo.GetAsNoTrackingQuery().Where(z => z.Id == transaction.BankId.Value).FirstOrDefaultAsync()).Name;
                var row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(transaction.CreatedAt.Value.ToString("HH:ss").ToPersianNumber(), $"outboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData(bankName, $"outboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData($"{transaction.Amount.ToString("N0").ToPersianNumber()} تومان", $"outboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData((((currentPage - 1) * rowsPerPage) + counter).ToString().ToPersianNumber(), $"outboundtransaction_{transaction.Id}"),
                };
                inlineKeyboardButtons.Add(row);
            }

            currentPage = currentPage == 0 ? 1 : currentPage;
            // Add pagination buttons
            var paginationButtons = new List<InlineKeyboardButton>();
            if (currentPage > 1)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData(" صفحه قبل »", $"outboundpage_{currentPage - 1}"));
            }
            if (totalCount >= currentPage * rowsPerPage)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData("« صفحه بعد", $"outboundpage_{currentPage + 1}"));
            }
            if (paginationButtons.Any())
            {
                inlineKeyboardButtons.Add(paginationButtons);
            }
            var globalbuttons = new List<InlineKeyboardButton>();
            globalbuttons.AddRange(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,
            });
            inlineKeyboardButtons.Add(globalbuttons);
            return new InlineKeyboardMarkup(inlineKeyboardButtons);
        }

        //yesterday outbound
        public async Task<InlineKeyboardMarkup> GenerateYesterdayOutBoundPagination(List<TransactionDTO> transactions, int currentPage, int totalCount)
        {
            int rowsPerPage = 5;
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();


            int counter = 0;
            foreach (var transaction in transactions)
            {
                counter++;
                var bankName = (await _bankRepo.GetAsNoTrackingQuery().Where(z => z.Id == transaction.BankId.Value).FirstOrDefaultAsync()).Name;
                var row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(transaction.CreatedAt.Value.ToString("HH:ss").ToPersianNumber(), $"outboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData(bankName, $"outboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData($"{transaction.Amount.ToString("N0").ToPersianNumber()} تومان", $"outboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData((((currentPage - 1) * rowsPerPage) + counter).ToString().ToPersianNumber(), $"outboundtransaction_{transaction.Id}"),
                };
                inlineKeyboardButtons.Add(row);
            }

            currentPage = currentPage == 0 ? 1 : currentPage;
            // Add pagination buttons
            var paginationButtons = new List<InlineKeyboardButton>();
            if (currentPage > 1)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData(" صفحه قبل »", $"outboundyesterdaypage_{currentPage - 1}"));
            }
            if (totalCount >= currentPage * rowsPerPage)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData("« صفحه بعد", $"outboundyesterdaypage_{currentPage + 1}"));
            }
            if (paginationButtons.Any())
            {
                inlineKeyboardButtons.Add(paginationButtons);
            }
            var globalbuttons = new List<InlineKeyboardButton>();
            globalbuttons.AddRange(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,
            });
            inlineKeyboardButtons.Add(globalbuttons);
            return new InlineKeyboardMarkup(inlineKeyboardButtons);
        }

        public async Task<InlineKeyboardMarkup> GenerateInBoundPagination(List<TransactionDTO> transactions, int currentPage, int totalCount)
        {
            int rowsPerPage = 5;
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();


            int counter = 0;
            foreach (var transaction in transactions)
            {
                counter++;
                var bankName = (await _bankRepo.GetAsNoTrackingQuery().Where(z => z.Id == transaction.BankId.Value).FirstOrDefaultAsync()).Name;
                var row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(transaction.CreatedAt.Value.ToString("HH:ss").ToPersianNumber(), $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData(bankName, $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData($"{transaction.Amount.ToString("N0").ToPersianNumber()} تومان", $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData((((currentPage - 1) * rowsPerPage) + counter).ToString().ToPersianNumber(), $"inboundtransaction_{transaction.Id}"),
                };
                inlineKeyboardButtons.Add(row);
            }

            currentPage = currentPage == 0 ? 1 : currentPage;
            // Add pagination buttons
            var paginationButtons = new List<InlineKeyboardButton>();
            if (currentPage > 1)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData(" صفحه قبل »", $"inboundpage_{currentPage - 1}"));
            }
            if (totalCount >= currentPage * rowsPerPage)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData("« صفحه بعد", $"inboundpage_{currentPage + 1}"));
            }
            if (paginationButtons.Any())
            {
                inlineKeyboardButtons.Add(paginationButtons);
            }
            var globalbuttons = new List<InlineKeyboardButton>();
            globalbuttons.AddRange(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,
            });
            inlineKeyboardButtons.Add(globalbuttons);
            return new InlineKeyboardMarkup(inlineKeyboardButtons);
        }

        public async Task<InlineKeyboardMarkup> GenerateInBoundLastMonthPagination(List<TransactionDTO> transactions, int currentPage, int totalCount)
        {
            int rowsPerPage = 5;
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();


            int counter = 0;
            foreach (var transaction in transactions)
            {
                counter++;
                var bankName = (await _bankRepo.GetAsNoTrackingQuery().Where(z => z.Id == transaction.BankId.Value).FirstOrDefaultAsync()).Name;
                var row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(transaction.CreatedAt.Value.ToString("HH:ss").ToPersianNumber(), $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData(bankName, $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData($"{transaction.Amount.ToString("N0").ToPersianNumber()} تومان", $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData((((currentPage - 1) * rowsPerPage) + counter).ToString().ToPersianNumber(), $"inboundtransaction_{transaction.Id}"),
                };
                inlineKeyboardButtons.Add(row);
            }

            currentPage = currentPage == 0 ? 1 : currentPage;
            // Add pagination buttons
            var paginationButtons = new List<InlineKeyboardButton>();
            if (currentPage > 1)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData(" صفحه قبل »", $"inboundlastmonthpage_{currentPage - 1}"));
            }
            if (totalCount >= currentPage * rowsPerPage)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData("« صفحه بعد", $"inboundlastmonthpage_{currentPage + 1}"));
            }
            if (paginationButtons.Any())
            {
                inlineKeyboardButtons.Add(paginationButtons);
            }
            var globalbuttons = new List<InlineKeyboardButton>();
            globalbuttons.AddRange(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,
            });
            inlineKeyboardButtons.Add(globalbuttons);
            return new InlineKeyboardMarkup(inlineKeyboardButtons);
        }

        public async Task<InlineKeyboardMarkup> GenerateOutBoundLastMonthPagination(List<TransactionDTO> transactions, int currentPage, int totalCount)
        {
            int rowsPerPage = 5;
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();


            int counter = 0;
            foreach (var transaction in transactions)
            {
                counter++;
                var bankName = (await _bankRepo.GetAsNoTrackingQuery().Where(z => z.Id == transaction.BankId.Value).FirstOrDefaultAsync()).Name;
                var row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(transaction.CreatedAt.Value.ToString("HH:ss").ToPersianNumber(), $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData(bankName, $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData($"{transaction.Amount.ToString("N0").ToPersianNumber()} تومان", $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData((((currentPage - 1) * rowsPerPage) + counter).ToString().ToPersianNumber(), $"inboundtransaction_{transaction.Id}"),
                };
                inlineKeyboardButtons.Add(row);
            }

            currentPage = currentPage == 0 ? 1 : currentPage;
            // Add pagination buttons
            var paginationButtons = new List<InlineKeyboardButton>();
            if (currentPage > 1)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData(" صفحه قبل »", $"outboundlastmonthpage_{currentPage - 1}"));
            }
            if (totalCount >= currentPage * rowsPerPage)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData("« صفحه بعد", $"outboundlastmonthpage_{currentPage + 1}"));
            }
            if (paginationButtons.Any())
            {
                inlineKeyboardButtons.Add(paginationButtons);
            }
            var globalbuttons = new List<InlineKeyboardButton>();
            globalbuttons.AddRange(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,
            });
            inlineKeyboardButtons.Add(globalbuttons);
            return new InlineKeyboardMarkup(inlineKeyboardButtons);
        }


        public async Task<InlineKeyboardMarkup> GenerateInBoundLastWeekPagination(List<TransactionDTO> transactions, int currentPage, int totalCount)
        {
            int rowsPerPage = 5;
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();


            int counter = 0;
            foreach (var transaction in transactions)
            {
                counter++;
                var bankName = (await _bankRepo.GetAsNoTrackingQuery().Where(z => z.Id == transaction.BankId.Value).FirstOrDefaultAsync()).Name;
                var row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(transaction.CreatedAt.Value.ToString("HH:ss").ToPersianNumber(), $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData(bankName, $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData($"{transaction.Amount.ToString("N0").ToPersianNumber()} تومان", $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData((((currentPage - 1) * rowsPerPage) + counter).ToString().ToPersianNumber(), $"inboundtransaction_{transaction.Id}"),
                };
                inlineKeyboardButtons.Add(row);
            }

            currentPage = currentPage == 0 ? 1 : currentPage;
            // Add pagination buttons
            var paginationButtons = new List<InlineKeyboardButton>();
            if (currentPage > 1)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData(" صفحه قبل »", $"inboundlastweekpage_{currentPage - 1}"));
            }
            if (totalCount >= currentPage * rowsPerPage)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData("« صفحه بعد", $"inboundlastweekpage_{currentPage + 1}"));
            }
            if (paginationButtons.Any())
            {
                inlineKeyboardButtons.Add(paginationButtons);
            }
            var globalbuttons = new List<InlineKeyboardButton>();
            globalbuttons.AddRange(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,
            });
            inlineKeyboardButtons.Add(globalbuttons);
            return new InlineKeyboardMarkup(inlineKeyboardButtons);
        }

        public async Task<InlineKeyboardMarkup> GenerateOutBoundLastWeekPagination(List<TransactionDTO> transactions, int currentPage, int totalCount)
        {
            int rowsPerPage = 5;
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();


            int counter = 0;
            foreach (var transaction in transactions)
            {
                counter++;
                var bankName = (await _bankRepo.GetAsNoTrackingQuery().Where(z => z.Id == transaction.BankId.Value).FirstOrDefaultAsync()).Name;
                var row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(transaction.CreatedAt.Value.ToString("HH:ss").ToPersianNumber(), $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData(bankName, $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData($"{transaction.Amount.ToString("N0").ToPersianNumber()} تومان", $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData((((currentPage - 1) * rowsPerPage) + counter).ToString().ToPersianNumber(), $"inboundtransaction_{transaction.Id}"),
                };
                inlineKeyboardButtons.Add(row);
            }

            currentPage = currentPage == 0 ? 1 : currentPage;
            // Add pagination buttons
            var paginationButtons = new List<InlineKeyboardButton>();
            if (currentPage > 1)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData(" صفحه قبل »", $"outboundlastweekpage_{currentPage - 1}"));
            }
            if (totalCount >= currentPage * rowsPerPage)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData("« صفحه بعد", $"outboundlastweekpage_{currentPage + 1}"));
            }
            if (paginationButtons.Any())
            {
                inlineKeyboardButtons.Add(paginationButtons);
            }
            var globalbuttons = new List<InlineKeyboardButton>();
            globalbuttons.AddRange(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,
            });
            inlineKeyboardButtons.Add(globalbuttons);
            return new InlineKeyboardMarkup(inlineKeyboardButtons);
        }


        public async Task<InlineKeyboardMarkup> GenerateInBoundYesterdayPagination(List<TransactionDTO> transactions, int currentPage, int totalCount)
        {
            int rowsPerPage = 5;
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();


            int counter = 0;
            foreach (var transaction in transactions)
            {
                counter++;
                var bankName = (await _bankRepo.GetAsNoTrackingQuery().Where(z => z.Id == transaction.BankId.Value).FirstOrDefaultAsync()).Name;
                var row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(transaction.CreatedAt.Value.ToString("HH:ss").ToPersianNumber(), $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData(bankName, $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData($"{transaction.Amount.ToString("N0").ToPersianNumber()} تومان", $"inboundtransaction_{transaction.Id}"),
                    InlineKeyboardButton.WithCallbackData((((currentPage - 1) * rowsPerPage) + counter).ToString().ToPersianNumber(), $"inboundtransaction_{transaction.Id}"),
                };
                inlineKeyboardButtons.Add(row);
            }

            currentPage = currentPage == 0 ? 1 : currentPage;
            // Add pagination buttons
            var paginationButtons = new List<InlineKeyboardButton>();
            if (currentPage > 1)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData(" صفحه قبل »", $"inboundyesterdaypage_{currentPage - 1}"));
            }
            if (totalCount >= currentPage * rowsPerPage)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData("« صفحه بعد", $"inboundyesterdaypage_{currentPage + 1}"));
            }
            if (paginationButtons.Any())
            {
                inlineKeyboardButtons.Add(paginationButtons);
            }
            var globalbuttons = new List<InlineKeyboardButton>();
            globalbuttons.AddRange(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,
            });
            inlineKeyboardButtons.Add(globalbuttons);
            return new InlineKeyboardMarkup(inlineKeyboardButtons);
        }



        public async Task SendInboundTransactionDetail(long chatId, int messageId, TransactionDTO transaction)
        {
            var bankName = (await _bankRepo.GetAsNoTrackingQuery().Where(z => z.Id == transaction.BankId.Value).FirstOrDefaultAsync()).Name;
            var CatName = (await _catRepo.GetAsNoTrackingQuery().Where(z => z.Id == transaction.CategoryId.Value).FirstOrDefaultAsync()).Name;
            var message = new StringBuilder($"{ConstMessage.OutboundTransactionPreview} \n  مبلغ: {transaction.Amount.ToString("N0").ToPersianNumber()}\n" +
                          $"بابت: {transaction.Description}\n" +
                          $"دسته بندی: {CatName}\n" +
                          $"ساعت: {transaction.CreatedAt.Value.ToString("HH:ss").ToPersianNumber()}\n" +
                          $"بانک: {bankName}\n" +
                           $"روز: {transaction.CreatedAt.Value.DayOfWeek.ConvertDayOfWeekToPersian()}\n" +
                          $"نوع: {transaction.Type.ToDisplay()}\n");
            var globalbuttons = new List<InlineKeyboardButton>();
            globalbuttons.AddRange(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,
            });
            var buttons = new InlineKeyboardMarkup(globalbuttons);
            await _botClient.EditMessageTextAsync(chatId, messageId, message.ToString(), replyMarkup: buttons);
        }

        public async Task SendOutboundTransactionDetail(long chatId, int messageId, TransactionDTO transaction)
        {
            var bankName = (await _bankRepo.GetAsNoTrackingQuery().Where(z => z.Id == transaction.BankId.Value).FirstOrDefaultAsync()).Name;
            var CatName = (await _catRepo.GetAsNoTrackingQuery().Where(z => z.Id == transaction.CategoryId.Value).FirstOrDefaultAsync()).Name;
            var message = new StringBuilder($"{ConstMessage.OutboundTransactionPreview} \n  مبلغ: {transaction.Amount.ToString("N0").ToPersianNumber()}\n" +
                          $"بابت: {transaction.Description}\n" +
                          $"دسته بندی: {CatName}\n" +
                          $"ساعت: {transaction.CreatedAt.Value.ToString("HH:ss").ToPersianNumber()}\n" +
                          $"بانک: {bankName}\n" +
                          $"روز: {transaction.CreatedAt.Value.DayOfWeek.ConvertDayOfWeekToPersian()}\n" +
                          $"نوع: {transaction.Type.ToDisplay()}\n");
            var globalbuttons = new List<InlineKeyboardButton>();
            globalbuttons.AddRange(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,
            });
            var buttons = new InlineKeyboardMarkup(globalbuttons);
            await _botClient.EditMessageTextAsync(chatId, messageId, message.ToString(), replyMarkup: buttons);
        }



        public async Task SendOutboundYesterday(long chatId, int MessageId, long telegramId, UserSession session)
        {
            var menu = new MenuConfigs(_botClient, _distributedCache);
            var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = 1, FromDate = DateTime.Now.AddDays(-1).Date, ToDate = DateTime.Now.AddDays(-1).Date, TelegramId = telegramId, Type = Domain.Enums.TransactionType.OutBound });
            if (data.Data.Count is 0)
            {
                var message = await _botClient.EditMessageTextAsync(chatId, MessageId, "<b>تراکنشی پرداختی برای دیروز وجود ندارد</b> ⚠", parseMode: ParseMode.Html);
                session.MessageIds.Add(message.MessageId);
                await Task.Delay(300);
                await menu.RollBackToMenu(telegramId, chatId, session);
            }
            else
            {
                var transactions = data.Data;
                var inlineKeyboard = await GenerateYesterdayOutBoundPagination(transactions, 1, data.TotalCount.Value);
                await _botClient.EditMessageTextAsync(chatId, MessageId, $"💸 <b>تراکنش‌های پرداختی دیروز</b> \n 📉 <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()} \n 📝 شماره صفحه:{1.ToString().ToPersianNumber()} </b> \n💰 <b>مجموع:{data.TotalAmount} تومان</b>", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineKeyboard);
            }
        }

        //dynamic report
        public async Task SendDynamicReportType(long chatId, int MessageId, DynamicReportType type)
        {
            switch (type)
            {
                case DynamicReportType.InboundToday:
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(ConstMessage.TransactionList, ConstCallBackData.Report.InBoundTodayReport),
                            InlineKeyboardButton.WithCallbackData(ConstMessage.Summary, ConstCallBackData.Report.InBoundTodaySummaryReport),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                        },
                    });
                    await _botClient
                       .EditMessageTextAsync(chatId, MessageId, ConstMessage.InboundChooseReportType, parseMode: ParseMode.Html,
                           replyMarkup: inlineKeyboard);
                    break;
                case DynamicReportType.InboundYesterday:
                    var inlineKeyboard1 = new InlineKeyboardMarkup(new[]
                   {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(ConstMessage.TransactionList, ConstCallBackData.Report.InBoundYesterdayReport),
                            InlineKeyboardButton.WithCallbackData(ConstMessage.Summary, ConstCallBackData.Report.InBoundYesterdaySummaryReport),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                        },
                    });
                    await _botClient
                       .EditMessageTextAsync(chatId, MessageId, ConstMessage.InboundChooseReportType, parseMode: ParseMode.Html,
                           replyMarkup: inlineKeyboard1);
                    break;
                case DynamicReportType.InboundLastWeek:
                    var inlineKeyboard2 = new InlineKeyboardMarkup(new[]
                 {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(ConstMessage.TransactionList, ConstCallBackData.Report.InBoundLastWeekReport),
                            InlineKeyboardButton.WithCallbackData(ConstMessage.Summary, ConstCallBackData.Report.InBoundLastWeekSummaryReport),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                        },
                    });
                    await _botClient
                       .EditMessageTextAsync(chatId, MessageId, ConstMessage.InboundChooseReportType, parseMode: ParseMode.Html,
                           replyMarkup: inlineKeyboard2);
                    break;
                case DynamicReportType.InboundLastMonth:
                    var inlineKeyboard3 = new InlineKeyboardMarkup(new[]
                    {
                             new[]
                             {
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.TransactionList, ConstCallBackData.Report.InBoundLastMonthReport),
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.Summary, ConstCallBackData.Report.InBoundLastMonthSummaryReport),
                             },
                             new[]
                             {
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                             },
                      });
                    await _botClient
                       .EditMessageTextAsync(chatId, MessageId, ConstMessage.InboundChooseReportType, parseMode: ParseMode.Html,
                           replyMarkup: inlineKeyboard3);
                    break;
                case DynamicReportType.OutboundToday:
                    var inlineKeyboard4 = new InlineKeyboardMarkup(new[]
                     {
                             new[]
                             {
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.TransactionList, ConstCallBackData.Report.OutBoundTodayReport),
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.Summary, ConstCallBackData.Report.OutBoundTodaySummaryReport),
                             },
                             new[]
                             {
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                             },
                      });
                    await _botClient
                       .EditMessageTextAsync(chatId, MessageId, ConstMessage.OutboundChooseReportType, parseMode: ParseMode.Html,
                           replyMarkup: inlineKeyboard4);
                    break;
                case DynamicReportType.OutboundYesterday:
                    var inlineKeyboard5 = new InlineKeyboardMarkup(new[]
                    {
                             new[]
                             {
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.TransactionList, ConstCallBackData.Report.OutBoundYesterdayReport),
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.Summary, ConstCallBackData.Report.OutBoundYesterdaySummaryReport),
                             },
                             new[]
                             {
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                             },
                      });
                    await _botClient
                       .EditMessageTextAsync(chatId, MessageId, ConstMessage.OutboundChooseReportType, parseMode: ParseMode.Html,
                           replyMarkup: inlineKeyboard5);
                    break;
                case DynamicReportType.OutboundLastWeek:
                    var inlineKeyboard6 = new InlineKeyboardMarkup(new[]
                   {
                             new[]
                             {
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.TransactionList, ConstCallBackData.Report.OutBoundLastWeekReport),
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.Summary, ConstCallBackData.Report.OutBoundLastWeekSummaryReport),
                             },
                             new[]
                             {
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                             },
                      });
                    await _botClient
                       .EditMessageTextAsync(chatId, MessageId, ConstMessage.OutboundChooseReportType, parseMode: ParseMode.Html,
                           replyMarkup: inlineKeyboard6);
                    break;
                case DynamicReportType.OutboundLastMonth:
                    var inlineKeyboard7 = new InlineKeyboardMarkup(new[]
                 {
                             new[]
                             {
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.TransactionList, ConstCallBackData.Report.OutBoundLastMonthReport),
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.Summary, ConstCallBackData.Report.OutBoundLastMonthSummaryReport),
                             },
                             new[]
                             {
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                             },
                      });
                    await _botClient
                       .EditMessageTextAsync(chatId, MessageId, ConstMessage.OutboundChooseReportType, parseMode: ParseMode.Html,
                           replyMarkup: inlineKeyboard7);
                    break;
                default:
                    break;
            }
        }

    }

    public enum DynamicReportType
    {
        InboundToday,
        InboundYesterday,
        InboundLastWeek,
        InboundLastMonth,
        OutboundToday,
        OutboundYesterday,
        OutboundLastWeek,
        OutboundLastMonth,
    }
}
