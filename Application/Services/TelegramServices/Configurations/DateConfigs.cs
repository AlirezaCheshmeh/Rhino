using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

class DateFunctions
{
    private static ITelegramBotClient _botClient;

    public DateFunctions(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async void SendDaysOfDate(long chatId)
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
                row.Add(InlineKeyboardButton.WithCallbackData(day.ToString(), day.ToString()));
            }
            keyboard.Add(row.ToArray());
        }

        var inlineKeyboard = new InlineKeyboardMarkup(keyboard);

        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "روز را انتخاب کنید:",
            replyMarkup: inlineKeyboard
        );
    }

    public async void SendMonthOfDate(long chatId)
    {
        string[] persianMonths = new string[]
        {
            "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
            "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
        };

        List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();

        for (int i = 0; i < persianMonths.Length; i++)
        {
            buttons.Add(InlineKeyboardButton.WithCallbackData(persianMonths[i], (i + 1).ToString()));
        }

        var inlineKeyboard = new InlineKeyboardMarkup(new[] { buttons.ToArray() });

        // Send the buttons
        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "ماه مورد نظر را انتخاب کنید:",
            replyMarkup: inlineKeyboard
        );
    }
}