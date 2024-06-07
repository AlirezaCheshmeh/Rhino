using Application.Services.TelegramServices.ConstVariable;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Application.Services.TelegramServices.Configurations
{
    public class SettingMenuConfigs
    {
        private readonly ITelegramBotClient _client;

        public SettingMenuConfigs(ITelegramBotClient client)
        {
            _client = client;
        }

        //setting 
        public async Task SendSettingMenuToUser(long chatId)
        {
            var inlineKeyboards = new InlineKeyboardMarkup(new[]
                        {
                            new[] {InlineKeyboardButton.WithCallbackData("🆕 ایجاد بانک جدید", ConstCallBackData.BankMenu.InsertNewBank)},
                            new[]
                            {
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu),
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel),
                            },
                        });
            await _client.SendTextMessageAsync(
            chatId: chatId,
            text: ConstMessage.Settings,
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboards);
        }


        public async Task<Message> SendChooseBankForInsertBankInSetting(long ChatId)
        {
            // Define the list of Iranian banks in Persian
            List<string> bankNames = new List<string>
    {
        "ملی",
        "سپه",
        "تجارت",
        "صادرات",
        "رفاه",
        "کشاورزی",
        "مسکن",
        "ملت",
        "پارسیان",
        "پاسارگاد",
        "شهر",
        "سینا",
        "توسعه تعاون",
        "دی",
        "اقتصاد نوین",
        "انصار",
        "حکمت ایرانیان",
        "ایران زمین",
        "کارآفرین",
        "سرمایه",
        "گردشگری",
        "آینده",
    };


            // Create inline keyboard buttons for the banks
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();
            for (int i = 0; i < bankNames.Count; i += 4)
            {
                var row = new List<InlineKeyboardButton>();
                for (int j = 0; j < 4 && i + j < bankNames.Count; j++)
                {
                    string bankName = bankNames[i + j];
                    string callbackData = GetEnglishCallbackData(bankName);
                    row.Add(InlineKeyboardButton.WithCallbackData(bankName, $"insertbank_{callbackData}"));
                }
                inlineKeyboardButtons.Add(row);
            }

            // Add the back button in a separate row
            inlineKeyboardButtons.Add(new List<InlineKeyboardButton>
    {
        InlineKeyboardButton.WithCallbackData(ConstMessage.Back, ConstCallBackData.Global.Back)
    });

            var inlineKeyboardInsertBank = new InlineKeyboardMarkup(inlineKeyboardButtons);

            // Send the message with the inline keyboard
            return await _client.SendTextMessageAsync(ChatId, text: ConstMessage.InsertNewBank, parseMode: ParseMode.Html, replyMarkup: inlineKeyboardInsertBank);
        }

        public  // Method to map Persian bank names to English callback data names
            string GetEnglishCallbackData(string persianName)
        {
            return persianName switch
            {
                "ملی" => "BankMelliIran",
                "سپه" => "BankSepah",
                "تجارت" => "BankTejarat",
                "صادرات" => "BankSaderatIran",
                "رفاه" => "BankRefah",
                "کشاورزی" => "BankKeshavarziIran",
                "مسکن" => "BankMaskan",
                "ملت" => "BankMellat",
                "پارسیان" => "BankParsian",
                "پاسارگاد" => "BankPasargad",
                "شهر" => "BankShahr",
                "سینا" => "BankSina",
                "توسعه تعاون" => "BankToseeTavon",
                "دی" => "BankDey",
                "اقتصاد نوین" => "BankEghtesadNovin",
                "انصار" => "BankAnsar",
                "حکمت ایرانیان" => "BankHekmatIranian",
                "ایران زمین" => "BankIranzamin",
                "کارآفرین" => "BankKarafarin",
                "سرمایه" => "BankSarmayeh",
                "گردشگری" => "BankTourism",
                "آینده" => "BankAyandeh",
                _ => "UnknownBank"
            };
        }


        public string GetPersianBankName(string englishName)
        {
            return englishName switch
            {
                "BankMelliIran" => "ملی",
                "BankSepah" => "سپه",
                "BankTejarat" => "تجارت",
                "BankSaderatIran" => "صادرات",
                "BankRefah" => "رفاه",
                "BankKeshavarziIran" => "کشاورزی",
                "BankMaskan" => "مسکن",
                "BankMellat" => "ملت",
                "BankParsian" => "پارسیان",
                "BankPasargad" => "پاسارگاد",
                "BankShahr" => "شهر",
                "BankSina" => "سینا",
                "BankToseeTavon" => "توسعه تعاون",
                "BankDey" => "دی",
                "BankEghtesadNovin" => "اقتصاد نوین",
                "BankAnsar" => "انصار",
                "BankHekmatIranian" => "حکمت ایرانیان",
                "BankIranzamin" => "ایران زمین",
                "BankKarafarin" => "کارآفرین",
                "BankSarmayeh" => "سرمایه",
                "BankTourism" => "گردشگری",
                "BankAyandeh" => "آینده",
                _ => "نامشخص"
            };
        }

    }
}
