using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

public class DateConfigs
{
    private static ITelegramBotClient _botClient;

    public DateConfigs(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    // Function to send days of the month as buttons
    public async Task SendDaysOfDate(long chatId)
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

    public async Task SendMonthOfDate(long chatId)
    {
        // List of Persian months
        string[] persianMonths = new string[]
        {
            "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
            "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
        };

        List<InlineKeyboardButton[]> keyboard = new List<InlineKeyboardButton[]>();

        for (int i = 0; i < persianMonths.Length; i += 6)
        {
            List<InlineKeyboardButton> row = new List<InlineKeyboardButton>();

            for (int j = 0; j < 6 && (i + j) < persianMonths.Length; j++)
            {
                var month = persianMonths[i + j];
                var index = i + j + 1; 

                row.Add(InlineKeyboardButton.WithCallbackData(month, index.ToString()));
            }
            keyboard.Add(row.ToArray());
        }

        var inlineKeyboard = new InlineKeyboardMarkup(keyboard);

        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "ماه مورد نظر را انتخاب کنید:", 
            replyMarkup: inlineKeyboard
        );
    }
}