using Application.Extensions;
using Application.Services.TelegramServices.ConstVariable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using static Application.Services.TelegramServices.ConstVariable.ConstCallBackData;
using Telegram.Bot.Types.ReplyMarkups;
using Application.Services.TelegramServices.BaseMethods;
using Telegram.Bot.Types;

namespace Application.Services.TelegramServices.Configurations
{
    public class ReminderConfigs
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public ReminderConfigs(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }

        public async Task SendChooseMonthReminder(long chatId)
        {
            string[] persianMonths = new string[]
        {
            "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
            "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
        };

            List<List<InlineKeyboardButton>> buttonRows = new List<List<InlineKeyboardButton>>();

            for (int i = 0; i < persianMonths.Length; i += 2)
            {
                var row = new List<InlineKeyboardButton>
            {
              InlineKeyboardButton.WithCallbackData($"{NumbersConvertorExtension.ToPersianNumber((i + 2).ToString())}-{persianMonths[i + 1]}", ConstCallBackData.Reminder.ChooseReminderMonth +"-"+ (i + 2).ToString())
            };

                if (i + 1 < persianMonths.Length)
                {
                    row.Add(InlineKeyboardButton.WithCallbackData($"{NumbersConvertorExtension.ToPersianNumber((i + 1).ToString())}-{persianMonths[i]}", ConstCallBackData.Reminder.ChooseReminderMonth + "-" + (i + 1).ToString()));
                }

                buttonRows.Add(row);
            }
            buttonRows.AddRange(new List<List<InlineKeyboardButton>>
            {
                new List<InlineKeyboardButton>
                {
                     InlineKeyboardButton.WithCallbackData(ConstMessage.Back, Global.Back) ,
                     InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, OutboundTransactionPreview.Cancel) ,
                }
            });

            var inlineKeyboard = new InlineKeyboardMarkup(buttonRows);

            // Send the buttons
            await _telegramBotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "ماه مورد نظر را برای یادآوری انتخاب کنید:",
                replyMarkup: inlineKeyboard
            );
        }


        public async Task SendChooseDayReminder(long chatId)
        {
            const int rows = 5;
            int daysInMonth = 0;
            var month = DateTime.Now.Month;
            if (month > 6)
                daysInMonth = 30;
            else
                daysInMonth = 31;

            List<InlineKeyboardButton[]> keyboard = new List<InlineKeyboardButton[]>();

            for (int i = 1; i <= daysInMonth; i += rows)
            {
                List<InlineKeyboardButton> row = new List<InlineKeyboardButton>();
                for (int j = 0; j < rows && (i + j) <= daysInMonth; j++)
                {
                    var day = i + j;
                    row.Add(InlineKeyboardButton.WithCallbackData(day.ToString().ToPersianNumber(), ConstCallBackData.Reminder.ChooseReminderDay + "-" + day.ToString()));
                }
                keyboard.Add(row.ToArray());
            }
            List<InlineKeyboardButton> rowGlobal = new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, OutboundTransactionPreview.Cancel),
            InlineKeyboardButton.WithCallbackData(ConstMessage.Back, Global.Back)
        };
            keyboard.Add(rowGlobal.ToArray());


            var inlineKeyboard = new InlineKeyboardMarkup(keyboard);

            await _telegramBotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "روز یادآوری را انتخاب کنید:",
                replyMarkup: inlineKeyboard
            );
        }



        public async Task<Message> SendReminderInsertAmount(long chatId)
        {

            var reminderbtns = new InlineKeyboardMarkup(new[]
            {
                new[] {InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back) },
            });
            return await _telegramBotClient.SendTextMessageAsync(
               chatId: chatId,
               text: ConstMessage.InsertReminderAmount,
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               replyMarkup: reminderbtns
           );
        }


        public async Task SendReminderInsertDescription(long chatId)
        {
            var reminderbtns = new InlineKeyboardMarkup(new[]
           {
                new[] {InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back) },
            });
            await _telegramBotClient.SendTextMessageAsync(
               chatId: chatId,
               text: ConstMessage.InsertReminderDescription,
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               replyMarkup: reminderbtns
           );
        }

        public async Task SendReminderPreviewAsync(long chatId, ReminderDto reminder)
        {
            var message = "⚠ یادآوری ایجاد شده شما: \n " +
                $"تاریخ : {DateExtension.ConvertToPersianDate(reminder.RemindDate.ToString("yyy/MM/dd"))}  \n" +
                $"مبلغ : {reminder.Amount.ToString("N0").ToPersianNumber()} \n" +
                $"بابت : {reminder.Description} \n";

            var inlineKeyboards = new InlineKeyboardMarkup(new[]
            {
               new[]
               {
                   InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.Global.BackToMenu),
                   InlineKeyboardButton.WithCallbackData(ConstMessage.Submit,ConstCallBackData.ReminderPreview.Submit)
               },
            });
            await _telegramBotClient.SendTextMessageAsync(chatId: chatId, text: message.ToString(), replyMarkup: inlineKeyboards);
        }
    }



}
