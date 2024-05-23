using Application.Extensions;
using Application.Services.TelegramServices.ConstVariable;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using static Application.Services.TelegramServices.ConstVariable.ConstCallBackData;

class DateFunctions
{
    private static ITelegramBotClient _botClient;

    public DateFunctions(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task SendInboundDaysOfDate(long chatId)
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
                row.Add(InlineKeyboardButton.WithCallbackData(day.ToString().ToPersianNumber(), ConstCallBackData.InboundTransaction.InBoundSpeseficDay + "-" + day.ToString()));
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

        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "روز را انتخاب کنید:",
            replyMarkup: inlineKeyboard
        );
    }


    public async Task SendOutboundDaysOfDate(long chatId)
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
                row.Add(InlineKeyboardButton.WithCallbackData(day.ToString().ToPersianNumber(), ConstCallBackData.OutboundTransaction.OutBoundSpeseficDay + "-" + day.ToString()));
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

        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "روز را انتخاب کنید:",
            replyMarkup: inlineKeyboard
        );
    }

    public async Task SendInBoundMonthOfDate(long chatId)
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
              InlineKeyboardButton.WithCallbackData($"{NumbersConvertorExtension.ToPersianNumber((i + 2).ToString())}-{persianMonths[i + 1]}", ConstCallBackData.InboundTransaction.InboundSpeseficMonth +"-"+ (i + 2).ToString())
            };

            if (i + 1 < persianMonths.Length)
            {
                row.Add(InlineKeyboardButton.WithCallbackData($"{NumbersConvertorExtension.ToPersianNumber((i + 1).ToString())}-{persianMonths[i]}", ConstCallBackData.InboundTransaction.InboundSpeseficMonth + "-" + (i + 1).ToString()));
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
        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "ماه مورد نظر را انتخاب کنید:",
            replyMarkup: inlineKeyboard
        );
    }


    public async Task SendOutBoundMonthOfDate(long chatId)
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
              InlineKeyboardButton.WithCallbackData($"{NumbersConvertorExtension.ToPersianNumber((i + 2).ToString())}-{persianMonths[i + 1]}", ConstCallBackData.OutboundTransaction.OutBoundSpeseficMonth +"-"+ (i + 2).ToString())
            };

            if (i + 1 < persianMonths.Length)
            {
                row.Add(InlineKeyboardButton.WithCallbackData($"{NumbersConvertorExtension.ToPersianNumber((i + 1).ToString())}-{persianMonths[i]}", ConstCallBackData.OutboundTransaction.OutBoundSpeseficMonth + "-" + (i + 1).ToString()));
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
        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "ماه مورد نظر را انتخاب کنید:",
            replyMarkup: inlineKeyboard
        );
    }
}