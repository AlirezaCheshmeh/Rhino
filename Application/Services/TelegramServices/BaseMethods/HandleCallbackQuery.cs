using Application.Services.CacheServices;
using Application.Utility;
using Domain.Entities.Transactions;
using Domain.Enums;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Application.Services.TelegramServices.BaseMethods.HandleUpdate;
using Application.Services.TelegramServices.ConstVariable;
using Application.Services.TelegramServices.Configurations;
using Application.Services.TelegramServices.Configurations.Base;
using Application.Services.TelegramServices.Configurations.Commands;
using Application.Cqrs.Commands;
using Application.Mediator.Transactions.Command;
using AutoMapper;
using Application.Mediator.Transactions.DTOs;
using Application.Common;
using Domain.Entities.Banks;
using Domain.Entities.Categories;
using Domain.Entities.Plans;
using Domain.Entities.UserPurchases;
using Application.Services.TelegramServices.Interfaces;
using Application.Extensions;
using Microsoft.Extensions.DependencyInjection;
using static Application.Services.TelegramServices.ConstVariable.ConstCallBackData;
using Application.Mediator.Reminders.DTOs;
using Application.Mediator.Reminders.Command;
using Application.Cqrs.Queris;
using Application.Mediator.Transactions.Query;
using Domain.Entities.Reminders;
using Microsoft.EntityFrameworkCore;
using Application.Mediator.Banks.Command;
using Application.Mediator.Banks.DTOs;

namespace Application.Services.TelegramServices.BaseMethods
{
    public class HandleCallbackQuery : IHandleCallbackQuery, IScopedDependency
    {
        private readonly ICacheServices _cache;
        private readonly IDistributedCache _disCache;
        private readonly IDynamicButtonsServices _dynamicButtonServices;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly IGenericRepository<Bank> _bankRepo;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IGenericRepository<Plan> _planRepo;
        private readonly IGenericRepository<UserPurchase> _userPerchaseRepo;
        private readonly IGenericRepository<Domain.Entities.Reminders.Reminder> _reminderRepository;
        private readonly IMapper _mapper;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public HandleCallbackQuery(ICacheServices cache, IDistributedCache disCache, IDynamicButtonsServices dynamicButtonServices, ICommandDispatcher commandDispatcher, IMapper mapper, IGenericRepository<Bank> bankRepo, IGenericRepository<Category> categoryRepo, IGenericRepository<Plan> planRepo, IGenericRepository<UserPurchase> userPerchaseRepo, IServiceScopeFactory serviceScopeFactory, IQueryDispatcher queryDispatcher, IGenericRepository<Domain.Entities.Reminders.Reminder> reminderRepository)
        {
            _cache = cache;
            _disCache = disCache;
            _dynamicButtonServices = dynamicButtonServices;
            _commandDispatcher = commandDispatcher;
            _mapper = mapper;
            _bankRepo = bankRepo;
            _categoryRepo = categoryRepo;
            _planRepo = planRepo;
            _userPerchaseRepo = userPerchaseRepo;
            _serviceScopeFactory = serviceScopeFactory;
            _queryDispatcher = queryDispatcher;
            _reminderRepository = reminderRepository;
        }
        public async Task HandleCallbackQueryAsync(ITelegramBotClient client, CallbackQuery callbackQuery, UserSession userSession)
        {
            MenuConfigs menu = new(client, _disCache);
            TransactionConfigs transactionMenu = new(client, _dynamicButtonServices, _bankRepo, _categoryRepo);
            SettingMenuConfigs settingMenu = new(client);
            CategoryConfigs categoryMenu = new(client, _dynamicButtonServices, _categoryRepo, _serviceScopeFactory);
            GlobalConfigs globalMessage = new(client);
            PlanConfigs planConfig = new(client, _planRepo);
            AccountConfigs accountConfig = new(_userPerchaseRepo);
            DateFunctions dateConfig = new(client);
            ReminderConfigs reminderConfig = new(client);
            ReportConfigs reportConfig = new(client, _queryDispatcher, _disCache, _bankRepo, _categoryRepo);
            try
            {
                var userIdKey = callbackQuery.From.Id;
                if (callbackQuery.Message is null)
                {
                    var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                    userSession.MessageIds.Add(ErrorMessage.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                    await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                }

                if (userSession.CommandState is not CommandState.Init)
                {
                    userSession.MessageIds.Add(callbackQuery.Message.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                }
                //insert bank
                if (callbackQuery.Data.StartsWith("insertbank_"))
                {
                    var bankName =settingMenu.GetPersianBankName(callbackQuery.Data.Split('_')[1]);
                    //insert to db
                    var mappedBank = _mapper.Map<BankDTO>(new Bank
                    {
                        Branch = "",
                        Name = bankName,
                        SVG = "",
                        TelegramId = userIdKey
                    });
                    var resultBank = await _commandDispatcher.SendAsync(new InsertBankCommand { dto = mappedBank }, cancellationToken: default);
                    if (resultBank.IsSuccess)
                    {
                        var BankInsertedMessage = await client
                           .SendTextMessageAsync(callbackQuery.Message.Chat.Id, ConstMessage.Success, parseMode: ParseMode.Html);
                        userSession.MessageIds.Add(BankInsertedMessage.MessageId);
                        Task.Delay(250).Wait();
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                    }
                    else
                    {
                        var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                        userSession.MessageIds.Add(ErrorMessage.MessageId);
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                    }
                }
                //handle reminder
                if (callbackQuery.Data.StartsWith("remindmeagain-"))
                {
                    var reminderId = long.Parse(callbackQuery.Data.Split('-')[1]);
                    var reminder = await _reminderRepository.GetQuery().Where(z => z.Id == reminderId).FirstOrDefaultAsync();
                    reminder.IsExpire = false;
                    await _reminderRepository.UpdateAsync(reminder, cancellationToken: default);
                    var message = await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "✔️ <b>متوجه شدم، مجدد یادآوری میکنم</b>", parseMode: ParseMode.Html);
                    var reminderMessageId = await CacheExtension.GetValueAsync<int>(userIdKey + ConstKey.ReminderMessageId);
                    await Task.Delay(500);
                    await client.DeleteMessageAsync(callbackQuery.Message.Chat.Id, message.MessageId);
                    await client.DeleteMessageAsync(callbackQuery.Message.Chat.Id, reminderMessageId);
                    await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                }
                //handle pagination
                //outbound
                if (callbackQuery.Data.StartsWith("outboundpage_"))
                {
                    var pageNumber = int.Parse(callbackQuery.Data.Split('_')[1]);
                    var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = pageNumber, FromDate = DateTime.Now.Date, ToDate = DateTime.Now.Date, TelegramId = userIdKey, Type = TransactionType.OutBound });
                    var transactions = data.Data;
                    var pagination = await reportConfig.GenerateOutBoundPagination(transactions, pageNumber, data.TotalCount.Value);

                    var res1 = await client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, $"💸 <b>تراکنش‌های پرداختی امروز</b> \n  📉 <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}</b>  \n  📝 <b>شماره صفحه:{pageNumber.ToString().ToPersianNumber()}</b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b>", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: pagination);
                    userSession.MessageIds.Add(res1.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                }//lastweek
                if (callbackQuery.Data.StartsWith("outboundlastweekpage_"))
                {
                    var pageNumber = int.Parse(callbackQuery.Data.Split('_')[1]);
                    var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = pageNumber, FromDate = DateTime.Now.AddDays(-7).Date, TelegramId = userIdKey, Type = TransactionType.OutBound });
                    var transactions = data.Data;
                    var pagination = await reportConfig.GenerateOutBoundLastWeekPagination(transactions, pageNumber, data.TotalCount.Value);

                    var res1 = await client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, $"💸 <b>تراکنش‌های پرداختی در هفته اخیر</b> \n  📉 <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}</b>  \n  📝 <b>شماره صفحه:{pageNumber.ToString().ToPersianNumber()}</b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b>", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: pagination);
                    userSession.MessageIds.Add(res1.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                }//yesterday
                if (callbackQuery.Data.StartsWith("outboundyesterdaypage_"))
                {
                    var pageNumber = int.Parse(callbackQuery.Data.Split('_')[1]);
                    var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = pageNumber, FromDate = DateTime.Now.AddDays(-1).Date, ToDate = DateTime.Now.AddDays(-1).Date, TelegramId = userIdKey, Type = TransactionType.OutBound });
                    var transactions = data.Data;
                    var pagination = await reportConfig.GenerateYesterdayOutBoundPagination(transactions, pageNumber, data.TotalCount.Value);

                    var res3 = await client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, $"💸 <b>تراکنش‌های پرداختی دیروز</b> \n  📉 <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}</b>  \n  📝 <b>شماره صفحه:{pageNumber.ToString().ToPersianNumber()}</b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b>", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: pagination);
                    userSession.MessageIds.Add(res3.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                }//inbound lastweek
                if (callbackQuery.Data.StartsWith("inboundlastweekpage_"))
                {
                    var pageNumber = int.Parse(callbackQuery.Data.Split('_')[1]);
                    var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = pageNumber, FromDate = DateTime.Now.AddDays(-7).Date, TelegramId = userIdKey, Type = TransactionType.InBound });
                    var transactions = data.Data;
                    var pagination = await reportConfig.GenerateInBoundLastWeekPagination(transactions, pageNumber, data.TotalCount.Value);

                    var res3 = await client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, $"💸 <b>تراکنش‌های دریافتی در هفته اخیر</b> \n  📉 <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}</b>  \n  📝 <b>شماره صفحه:{pageNumber.ToString().ToPersianNumber()}</b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b>", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: pagination);
                    userSession.MessageIds.Add(res3.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                }//inbound yesterday
                if (callbackQuery.Data.StartsWith("inboundyesterdaypage_"))
                {
                    var pageNumber = int.Parse(callbackQuery.Data.Split('_')[1]);
                    var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = pageNumber, FromDate = DateTime.Now.AddDays(-1).Date, ToDate = DateTime.Now.AddDays(-1).Date, TelegramId = userIdKey, Type = TransactionType.OutBound });
                    var transactions = data.Data;
                    var pagination = await reportConfig.GenerateInBoundYesterdayPagination(transactions, pageNumber, data.TotalCount.Value);

                    var res2 = await client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, $"💸 <b>تراکنش‌های پرداختی دیروز</b> \n  📉 <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}</b>  \n  📝 <b>شماره صفحه:{pageNumber.ToString().ToPersianNumber()}</b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b>", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: pagination);
                    userSession.MessageIds.Add(res2.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                }
                //inbound last month
                if (callbackQuery.Data.StartsWith("inboundlastmonthpage_"))
                {
                    var pageNumber = int.Parse(callbackQuery.Data.Split('_')[1]);
                    var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = pageNumber, FromDate = DateTime.Now.AddDays(-30).Date, TelegramId = userIdKey, Type = TransactionType.InBound });
                    var transactions = data.Data;
                    var pagination = await reportConfig.GenerateInBoundLastMonthPagination(transactions, pageNumber, data.TotalCount.Value);

                    var res2 = await client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, $"💸 <b>تراکنش‌های دریافتی در ماه اخیر</b> \n  📉 <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}</b>  \n  📝 <b>شماره صفحه:{pageNumber.ToString().ToPersianNumber()}</b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b>", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: pagination);
                    userSession.MessageIds.Add(res2.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                }
                //outbound lastmonth
                if (callbackQuery.Data.StartsWith("outboundlastmonthpage_"))
                {
                    var pageNumber = int.Parse(callbackQuery.Data.Split('_')[1]);
                    var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = pageNumber, FromDate = DateTime.Now.AddDays(-30).Date, TelegramId = userIdKey, Type = TransactionType.OutBound });
                    var transactions = data.Data;
                    var pagination = await reportConfig.GenerateOutBoundLastMonthPagination(transactions, pageNumber, data.TotalCount.Value);

                    var res2 = await client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, $"💸 <b>تراکنش‌های پرداختی در ماه اخیر</b> \n  📉 <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}</b>  \n  📝 <b>شماره صفحه:{pageNumber.ToString().ToPersianNumber()}</b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b>", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: pagination);
                    userSession.MessageIds.Add(res2.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                }
                //inbound
                if (callbackQuery.Data.StartsWith("inboundpage_"))
                {
                    var pageNumber = int.Parse(callbackQuery.Data.Split('_')[1]);
                    var data = await _queryDispatcher.SendAsync(new GetAllTransactionsQuery { Count = 5, PageNumber = pageNumber, FromDate = DateTime.Now.Date, ToDate = DateTime.Now.Date, TelegramId = userIdKey, Type = TransactionType.InBound });
                    var transactions = data.Data;
                    var pagination = await reportConfig.GenerateInBoundPagination(transactions, pageNumber, data.TotalCount.Value);
                    var globalbuttons = new List<InlineKeyboardButton>();
                    globalbuttons.AddRange(new List<InlineKeyboardButton> {
                      InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel) ,
                      InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back) ,
                    });
                    List<List<InlineKeyboardButton>> buttons = new();

                    var main = new InlineKeyboardMarkup(globalbuttons);
                    var res = await client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, $"💸 <b>تراکنش‌های دریافتی امروز</b> \n  📉 <b>تعداد کل:{data.TotalCount.ToString().ToPersianNumber()}</b>  \n  📝 <b>شماره صفحه:{pageNumber.ToString().ToPersianNumber()}</b>\n💰 <b>مجموع:{data.TotalAmount} تومان</b>", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: pagination);
                    userSession.MessageIds.Add(res.MessageId);
                    await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                }
                //detail
                //inbound
                if (callbackQuery.Data.StartsWith("inboundtransaction_"))
                {
                    var inId = long.Parse(callbackQuery.Data.Split('_')[1]);
                    var inboundTransactionDetail = await _queryDispatcher.SendAsync(new GetTransactionByIdQuery { Id = inId });
                    await reportConfig.SendInboundTransactionDetail(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, inboundTransactionDetail.Data);
                }
                //outbound
                if (callbackQuery.Data.StartsWith("outboundtransaction_"))
                {
                    var inId = long.Parse(callbackQuery.Data.Split('_')[1]);
                    var inboundTransactionDetail = await _queryDispatcher.SendAsync(new GetTransactionByIdQuery { Id = inId });
                    await reportConfig.SendOutboundTransactionDetail(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, inboundTransactionDetail.Data);
                }
                switch (userSession.CommandState)
                {

                    //global start===================
                    #region BackToMenu
                    case CommandState.BackToMenu:
                        await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        break;
                    #endregion
                    //global end=====================


                    //account start==================
                    #region BuyAccount
                    case CommandState.BuyAccount:
                        await planConfig.SendPlanToUser(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion
                    //account end====================

                    #region outbound
                    //outbound start=================
                    #region InsertOutBoundTransaction

                    case CommandState.InsertOutboundTransaction:
                        await transactionMenu.SendOutBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region OutboundTransactionDaily
                    case CommandState.OutboundTransactionDaily:
                        await transactionMenu.SendOutBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region OutboundSpeseficDate
                    case CommandState.OutboundTransactionSpecificDate:
                        await dateConfig.SendOutBoundMonthOfDate(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region OutBoundMonth
                    case CommandState.OutboundMonth:
                        var dateMonth = Convert.ToInt64(callbackQuery.Data?.Split("-").LastOrDefault());//get month here
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.OutBoundMonth, dateMonth);//set month to cache
                        await dateConfig.SendOutboundDaysOfDate(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region ChooseCategoryDaily
                    case CommandState.ChooseCategoryDaily:
                        DateTime? outBoundChosenDate = null;
                        if (callbackQuery.Data.Contains(ConstCallBackData.OutboundTransaction.OutBoundSpeseficDay))
                        {
                            var dateDay = Convert.ToInt32(callbackQuery.Data?.Split("-").LastOrDefault());//get day here
                            var outBountmonth = await CacheExtension.GetValueAsync<int>(userIdKey + ConstKey.OutBoundMonth);//set month to cache
                            var year1 = DateTime.Now.Year.ToPersianYear();
                            var chosenDate1 = $"{year1}/{outBountmonth}/{dateDay}";
                            outBoundChosenDate = DateExtension.ConvertToEnglishDate(chosenDate1);
                        }

                        var Outboundtransaction = new TransactionDto();
                        Outboundtransaction.Type = TransactionType.OutBound;
                        Outboundtransaction.CreatedAt = outBoundChosenDate;
                        Outboundtransaction.TelegramId = callbackQuery.From.Id;
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, Outboundtransaction);
                        await categoryMenu.SendOutBoundCategoriesToUser(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region ChooseBankDaily
                    case CommandState.ChooseBankDaily:
                        var cacheDate = await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        var catId = Convert.ToInt64(callbackQuery.Data?.Split("-").LastOrDefault());//todo: not found data and exception
                        if (cacheDate is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        else
                        {
                            cacheDate.CategoryId = catId;
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, cacheDate);
                            await transactionMenu.SendChooseBankAsync(callbackQuery.Message.Chat.Id, callbackQuery.From.Id);
                        }

                        break;
                    #endregion

                    #region Amount
                    case CommandState.Amount:
                        var cacheDateAmount = await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        var bankId = Convert.ToInt64(callbackQuery.Data?.Split("-").LastOrDefault());//todo not found data and exception
                        if (cacheDateAmount is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        cacheDateAmount!.BankId = bankId;
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, cacheDateAmount);
                        var inlineKeyboardAmount = new InlineKeyboardMarkup(new[]
                        {
                            new[] {InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu) },
                        });
                        var amountMessage = await client
                            .SendTextMessageAsync(callbackQuery.Message.Chat.Id, text: ConstMessage.OutBoundInsertAmount, parseMode: ParseMode.Html, replyMarkup: inlineKeyboardAmount);
                        userSession.MessageIds.Add(amountMessage.MessageId);
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        break;
                    #endregion

                    #region OutBoundTransactionSubmit
                    case CommandState.OutBoundTransactionSubmit:
                        TransactionDto? transactionSubmitted = null;
                        var value = await _disCache.GetAsync(userIdKey + ConstKey.Transaction);
                        if (value is { Length: > 0 })
                            transactionSubmitted = JsonSerializer.Deserialize<TransactionDto>(value);
                        if (transactionSubmitted is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        else
                        {
                            //insert to db
                            var mappedTransactionDto = _mapper.Map<TransactionDTO>(transactionSubmitted);
                            await _commandDispatcher.SendAsync(new InsertTransactionCommand { dto = mappedTransactionDto });
                            var descriptionMessage = await client
                                .SendTextMessageAsync(callbackQuery.Message.Chat.Id, ConstMessage.Success, parseMode: ParseMode.Html);
                            userSession.MessageIds.Add(descriptionMessage.MessageId);
                            Task.Delay(150).Wait();
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);

                        }
                        break;
                    #endregion

                    #region OutboundTransactionCancel
                    case CommandState.OutboundTransactionCancel:
                        var cancelMessage = await client
                            .SendTextMessageAsync(callbackQuery.Message.Chat.Id, ConstMessage.Cancel, parseMode: ParseMode.Html);
                        Task.Delay(150).Wait();
                        userSession.MessageIds.Add(cancelMessage.MessageId);
                        await _disCache.RemoveAsync(userIdKey + ConstKey.Transaction);
                        await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        break;
                    #endregion
                    //outbound end========================
                    #endregion

                    #region Inbound
                    //inbound start=======================
                    #region InsertInboundTransaction

                    case CommandState.InsertInboundTransaction:
                        await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        break;

                    #endregion

                    #region IntboundTransactionDaily
                    case CommandState.InboundTransactionDaily:
                        await transactionMenu.SendInBoundTransactionAsync(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region InboundChooseCategoryDaily
                    case CommandState.InboundChooseCategoryDaily:
                        DateTime? ininBoundChosenDate = null;
                        if (callbackQuery.Data.Contains(ConstCallBackData.InboundTransaction.InBoundSpeseficDay))
                        {
                            var IndateDay = Convert.ToInt32(callbackQuery.Data?.Split("-").LastOrDefault());//get day here
                            var InoutBountmonth = await CacheExtension.GetValueAsync<int>(userIdKey + ConstKey.InBoundMonth);//set month to cache
                            var Inyear = DateTime.Now.Year.ToPersianYear();
                            var InchosenDate = $"{Inyear}/{InoutBountmonth}/{IndateDay}";
                            ininBoundChosenDate = DateExtension.ConvertToEnglishDate(InchosenDate);
                        }
                        var Ininboundtransaction = new TransactionDto();
                        Ininboundtransaction.Type = TransactionType.InBound;
                        Ininboundtransaction.CreatedAt = ininBoundChosenDate;
                        Ininboundtransaction.TelegramId = callbackQuery.From.Id;
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, Ininboundtransaction);
                        await categoryMenu.SendInBoundCategoriesToUser(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion
                    #region InboundSpeseficDate
                    case CommandState.InboundTransactionSpecificDate:
                        await dateConfig.SendInBoundMonthOfDate(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region InboundMonth
                    case CommandState.InboundMonth:
                        var indateMonth = Convert.ToInt64(callbackQuery.Data?.Split("-").LastOrDefault());//get month here
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.InBoundMonth, indateMonth);//set month to cache
                        await dateConfig.SendInboundDaysOfDate(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion
                    #region InboundChooseBankDaily
                    case CommandState.InboundChooseBankDaily:
                        var InboundcacheDate = await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        var InboundcatId = Convert.ToInt64(callbackQuery.Data?.Split("-").LastOrDefault());//todo: not found data and exception
                        if (InboundcacheDate is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        else
                        {
                            InboundcacheDate.CategoryId = InboundcatId;
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, InboundcacheDate);
                            await transactionMenu.SendInBoundChooseBankAsync(callbackQuery.Message.Chat.Id, callbackQuery.From.Id);
                        }

                        break;
                    #endregion

                    #region InboundAmount
                    case CommandState.InboundAmount:
                        var InboundcacheDateAmount = await CacheExtension.GetValueAsync<TransactionDto>(userIdKey + ConstKey.Transaction);
                        var InboundbankId = Convert.ToInt64(callbackQuery.Data?.Split("-").LastOrDefault());//todo not found data and exception
                        if (InboundcacheDateAmount is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        InboundcacheDateAmount!.BankId = InboundbankId;
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Transaction, InboundcacheDateAmount);
                        var InboundinlineKeyboardAmount = new InlineKeyboardMarkup(new[]
                        {
                            new[] {InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu) },
                        });
                        var InboundamountMessage = await client
                            .SendTextMessageAsync(callbackQuery.Message.Chat.Id, text: ConstMessage.InsertAmount, parseMode: ParseMode.Html, replyMarkup: InboundinlineKeyboardAmount);
                        userSession.MessageIds.Add(InboundamountMessage.MessageId);
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        break;
                    #endregion

                    #region InboundTransactionSubmit
                    case CommandState.InBoundTransactionSubmit:
                        TransactionDto? InboundtransactionSubmitted = null;
                        var Inboundvalue = await _disCache.GetAsync(userIdKey + ConstKey.Transaction);
                        if (Inboundvalue is { Length: > 0 })
                            InboundtransactionSubmitted = JsonSerializer.Deserialize<TransactionDto>(Inboundvalue);
                        if (InboundtransactionSubmitted is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        else
                        {
                            //insert to db
                            var mappedTransactionDto = _mapper.Map<TransactionDTO>(InboundtransactionSubmitted);
                            await _commandDispatcher.SendAsync(new InsertTransactionCommand { dto = mappedTransactionDto });
                            var InbounddescriptionMessage = await client
                                .SendTextMessageAsync(callbackQuery.Message.Chat.Id, ConstMessage.Success, parseMode: ParseMode.Html);
                            userSession.MessageIds.Add(InbounddescriptionMessage.MessageId);
                            Task.Delay(150).Wait();
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        break;
                    #endregion

                    #region InboundTransactionCancel
                    case CommandState.InboundTransactionCancel:
                        var InboundcancelMessage = await client
                            .SendTextMessageAsync(callbackQuery.Message.Chat.Id, ConstMessage.Cancel, parseMode: ParseMode.Html);
                        Task.Delay(150).Wait();
                        userSession.MessageIds.Add(InboundcancelMessage.MessageId);
                        await _disCache.RemoveAsync(userIdKey + ConstKey.Transaction);
                        await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        break;
                    #endregion
                    //inbound end=========================
                    #endregion


                    //settings start======================
                    #region Settings

                    case CommandState.Settings:
                        await settingMenu.SendSettingMenuToUser(callbackQuery.Message.Chat.Id);
                        userSession.MessageIds.Add(callbackQuery.Message.MessageId);
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        break;
                    #endregion

                    #region InsertNewBank
                    case CommandState.InsertNewbank:
                        var bankMessage = await settingMenu.SendChooseBankForInsertBankInSetting(callbackQuery.Message.Chat.Id);
                        userSession.MessageIds.Add(callbackQuery.Message.MessageId);
                        userSession.MessageIds.Add(bankMessage.MessageId);
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        break;
                    #endregion
                    //settings end========================



                    //features satrt=====================
                    #region Reports
                    case CommandState.Reports:
                        var IsActive = await accountConfig.CheckUserActiveAccount(userIdKey);
                        if (!IsActive)
                        {
                            var message = await globalMessage.SendYouDontHaveActiveAccount(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(message.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        }
                        else
                        {
                            await reportConfig.SendChooseReportType(callbackQuery.Message.Chat.Id);
                        }
                        break;
                    #endregion

                    #region InboundReports
                    case CommandState.InboundReport:
                        await reportConfig.SendChooseTimeTypeInboundReport(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                        break;
                    #endregion

                    #region InboundTodayReportsDynamicType
                    case CommandState.InboundTodayReportsDynamicType:
                        await reportConfig.SendDynamicReportType(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, DynamicReportType.InboundToday);
                        break;
                    #endregion

                    #region InboundYesterdayReportsDynamicType
                    case CommandState.InboundYesterdayReportsDynamicType:
                        await reportConfig.SendDynamicReportType(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, DynamicReportType.InboundYesterday);
                        break;
                    #endregion

                    #region InboundYesterdaySummaryReport
                    case CommandState.InboundYesterdaySummary:
                        await reportConfig.SendInboundYesterdaySummary(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, userIdKey);
                        break;
                    #endregion

                    #region InboundLastWeekReportsDynamicType
                    case CommandState.InboundLastWeekReportsDynamicType:
                        await reportConfig.SendDynamicReportType(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, DynamicReportType.InboundLastWeek);
                        break;
                    #endregion

                    #region InboundLastWeekReports
                    case CommandState.InboundLastWeekReport:
                        await reportConfig.SendInboundLastWeek(callbackQuery.Message.Chat.Id, userIdKey, callbackQuery.Message.MessageId, userSession);
                        break;
                    #endregion

                    #region OutboundLastWeekReports
                    case CommandState.OutboundLastWeekReport:
                        await reportConfig.SendOutboundLastWeek(callbackQuery.Message.Chat.Id, userIdKey, callbackQuery.Message.MessageId, userSession);
                        break;
                    #endregion

                    #region InboundLastWeekSummaryReports
                    case CommandState.InboundLastWeekSummaryReport:
                        await reportConfig.SendInboundlastWeekSummary(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, userIdKey);
                        break;
                    #endregion

                    #region OutboundLastWeekSummaryReports
                    case CommandState.OutboundLastWeekSummaryReport:
                        await reportConfig.SendOutboundlastWeekSummary(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, userIdKey);
                        break;
                    #endregion

                    #region InboundLastMonthReportsDynamicType
                    case CommandState.InboundLastMonthReportsDynamicType:
                        await reportConfig.SendDynamicReportType(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, DynamicReportType.InboundLastMonth);
                        break;
                    #endregion

                    #region InboundLastMonthSummaryReport
                    case CommandState.InboundLastMonthSummaryReport:
                        await reportConfig.SendInboundlastMonthSummary(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, userIdKey);
                        break;
                    #endregion

                    #region OutboundLastMonthSummaryReport
                    case CommandState.OutboundLastMonthSummaryReport:
                        await reportConfig.SendOutboundlastMonthSummary(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, userIdKey);
                        break;
                    #endregion

                    #region OutboundLastMonthReport
                    case CommandState.OutboundLastMonthReport:
                        await reportConfig.SendOutboundLastMonth(callbackQuery.Message.Chat.Id, userIdKey, callbackQuery.Message.MessageId, userSession);
                        break;
                    #endregion

                    #region InboundLastMonthReport
                    case CommandState.InboundLastMonthReport:
                        await reportConfig.SendInboundLastMonth(callbackQuery.Message.Chat.Id, userIdKey, callbackQuery.Message.MessageId, userSession);
                        break;
                    #endregion

                    #region OutboundReports
                    case CommandState.OutboundReport:
                        await reportConfig.SendChooseTimeTypeOutboundReport(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                        break;
                    #endregion

                    #region OutboundTodayReportsDynamicType
                    case CommandState.OutboundTodayReportsDynamicType:
                        await reportConfig.SendDynamicReportType(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, DynamicReportType.OutboundToday);
                        break;
                    #endregion

                    #region OutboundYesterdayReportsDynamicType
                    case CommandState.OutboundYesterdayReportsDynamicType:
                        await reportConfig.SendDynamicReportType(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, DynamicReportType.OutboundYesterday);
                        break;
                    #endregion

                    #region OutboundYesterdayReport
                    case CommandState.OutboundYesterdayReport:
                        await reportConfig.SendOutboundYesterday(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, userIdKey, userSession);
                        break;
                    #endregion

                    #region OutboundLastWeekReportsDynamicType
                    case CommandState.OutboundLastWeekReportsDynamicType:
                        await reportConfig.SendDynamicReportType(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, DynamicReportType.OutboundLastWeek);
                        break;
                    #endregion

                    #region OutboundLastMonthReportsDynamicType
                    case CommandState.OutboundLastMonthReportsDynamicType:
                        await reportConfig.SendDynamicReportType(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, DynamicReportType.OutboundLastMonth);
                        break;
                    #endregion

                    #region OutboundTodayReport
                    case CommandState.OutboundTodayReport:
                        await reportConfig.SendOutboundToday(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, userIdKey, userSession);
                        break;

                    #endregion

                    #region InboundTodayReport
                    case CommandState.InboundTodayReport:
                        await reportConfig.SendInboundToday(callbackQuery.Message.Chat.Id, userIdKey, callbackQuery.Message.MessageId, userSession);
                        break;

                    #endregion

                    #region InboundYesterdayReportOu
                    case CommandState.InboundYesterdayReport:
                        await reportConfig.SendInboundYesterday(callbackQuery.Message.Chat.Id, userIdKey, callbackQuery.Message.MessageId, userSession);
                        break;

                    #endregion

                    #region OutboundTodaySummaryReport
                    case CommandState.OutboundSummary:
                        await reportConfig.SendOutboundTodaySummary(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, userIdKey);
                        break;
                    #endregion

                    #region OutboundYesterdaySummaryReport
                    case CommandState.OutboundYesterdaySummaryReport:
                        await reportConfig.SendOutboundYesterdaySummary(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, userIdKey);
                        break;
                    #endregion

                    #region InBoundTodaySummaryReports
                    case CommandState.InboundSummary:
                        await reportConfig.SendInboundTodaySummary(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, userIdKey);
                        break;
                    #endregion

                    #region PeriodicReminder
                    case CommandState.RemindPeriodic:
                        var IsActiveAccount = await accountConfig.CheckUserActiveAccount(userIdKey);
                        if (!IsActiveAccount)
                        {
                            var message = await globalMessage.SendYouDontHaveActiveAccount(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(message.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        }
                        else
                        {
                            await reminderConfig.SendChooseMonthReminder(callbackQuery.Message.Chat.Id);
                        }
                        break;
                    #endregion

                    #region ChooseDayReminder
                    case CommandState.ChooseDayReminder:
                        var rmeinderDateMonth = Convert.ToInt64(callbackQuery.Data?.Split("-").LastOrDefault());//get month here
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.ReminderMonth, rmeinderDateMonth);//set month to cache
                        await reminderConfig.SendChooseDayReminder(callbackQuery.Message.Chat.Id);
                        break;
                    #endregion

                    #region ReminderAmount
                    case CommandState.ReminderAmount:
                        var reminderdateDay = Convert.ToInt32(callbackQuery.Data?.Split("-").LastOrDefault());//get day here
                        var reminderMonth = await CacheExtension.GetValueAsync<int>(userIdKey + ConstKey.ReminderMonth);//set month to cache
                        var year = DateTime.Now.Year.ToPersianYear();
                        var chosenDate = $"{year}/{reminderMonth}/{reminderdateDay}";
                        var remindDate = DateExtension.ConvertToEnglishDate(chosenDate);
                        ReminderDto reminder = new();
                        reminder.Type = ReminderType.OneTime;
                        reminder.RemindDate = remindDate;
                        reminder.TelegramId = callbackQuery.From.Id;
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Reminder, reminder);
                        var messageReminder = await reminderConfig.SendReminderInsertAmount(callbackQuery.Message.Chat.Id);
                        userSession.MessageIds.Add(messageReminder.MessageId);
                        await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                        break;
                    #endregion


                    #region ReminderAccept
                    case CommandState.ReminderSubmit:
                        ReminderDto? ReminderSubmitted = null;
                        var cacheData = await _disCache.GetAsync(userIdKey + ConstKey.Reminder);
                        if (cacheData is { Length: > 0 })
                            ReminderSubmitted = JsonSerializer.Deserialize<ReminderDto>(cacheData);
                        if (ReminderSubmitted is null)
                        {
                            var ErrorMessage = await globalMessage.SendErrorToUser(callbackQuery.Message.Chat.Id);
                            userSession.MessageIds.Add(ErrorMessage.MessageId);
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        else
                        {
                            //insert to db
                            var mappedReminderDto = _mapper.Map<ReminderDTO>(ReminderSubmitted);
                            mappedReminderDto.ChatId = callbackQuery.Message.Chat.Id;
                            await _commandDispatcher.SendAsync(new InsertReminderCommand { dto = mappedReminderDto });
                            var InbounddescriptionMessage = await client
                                .SendTextMessageAsync(callbackQuery.Message.Chat.Id, ConstMessage.Success, parseMode: ParseMode.Html);
                            userSession.MessageIds.Add(InbounddescriptionMessage.MessageId);
                            Task.Delay(150).Wait();
                            await CacheExtension.UpdateValueAsync(userIdKey + ConstKey.Session, userSession);
                            await menu.RollBackToMenu(userIdKey, callbackQuery.Message.Chat.Id, userSession);
                        }
                        break;
                        #endregion
                        //features end=====================


                }


                // Acknowledge the callback query to prevent any repeat calls
                await client.AnswerCallbackQueryAsync(callbackQuery.Id);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }

    public class TransactionDto
    {
        public decimal? Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public TransactionType? Type { get; set; }
        public string? Description { get; set; }
        public long? TelegramId { get; set; }
        public long? MessageId { get; set; }
        public long? BankId { get; set; }
        public string? BankName { get; set; }
        public long? CategoryId { get; set; }
        public DateTime PayDateTime { get; set; } = DateTime.Now;
    }

    public class ReminderDto
    {
        public string Description { get; set; }
        public ReminderType Type { get; set; }
        public bool IsExpire { get; set; }
        public decimal Amount { get; set; }
        public DateTime RemindDate { get; set; }
        public bool IsRemindMeAgain { get; set; }
        public DateTime? RemindAgainDate { get; set; }
        public long TelegramId { get; set; }
    }
}

