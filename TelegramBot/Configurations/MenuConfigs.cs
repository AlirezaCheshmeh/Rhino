using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.ConstMessages;
using TelegramBot.ConstVariable;
using static TelegramBot.BaseMethods.HandleUpdate;
using static TelegramBot.ConstVariable.ConstCallBackData;

namespace TelegramBot.Configurations
{
    public class MenuConfigs
    {
        private readonly ITelegramBotClient _client;
        private readonly IDistributedCache _disCache;

        public MenuConfigs(ITelegramBotClient client, IDistributedCache disCache)
        {
            _client = client;
            _disCache = disCache;
        }
        public async Task SendMenuToUserAsync(long chatId)
        {
            var inlineKeyboards = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                 InlineKeyboardButton
                                     .WithCallbackData("🆕 ایجاد پرداختی جدید", ConstCallBackData.Menu.OutboundTransaction),
                            },
                            new[]
                            {
                                InlineKeyboardButton
                                    .WithCallbackData("🗞 ایجاد دریافتی جدید", ConstCallBackData.Menu.InboundTransaction),
                            },
                            new[]
                            {
                                InlineKeyboardButton
                                    .WithCallbackData("🔔 یادآوری رویداد یکروز ", ConstCallBackData.Menu.OnceReminder),
                                InlineKeyboardButton
                                    .WithCallbackData("🗓 یادآوری رویداد روزانه", ConstCallBackData.Menu.PeriodicReminder),

                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("⚙ تنظیمات",ConstCallBackData.Menu.Settings),
                                InlineKeyboardButton.WithCallbackData("🧮 ماشین حساب", ConstCallBackData.Menu.Calculator),

                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("⁉️ راهنما", ConstCallBackData.Menu.Guide),
                                InlineKeyboardButton.WithCallbackData("💬 پشتیبانی",ConstCallBackData.Menu.Supporter),

                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("💳 خرید اشتراک", ConstCallBackData.Menu.BuyAccount),
                            }
                        });
            await _client.SendTextMessageAsync(
            chatId: chatId,
            text: ConstMessage.Menu,
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboards);
        }


        public async Task RollBackToMenu(long userIdKey,long chatId,UserSession session)
        {
            session.MessageIds.RemoveAll(i => i == session.MessageIds.First());
            session.MessageIds.ForEach(item =>
            {
                _client.DeleteMessageAsync(chatId, item);
            });
            Task.Delay(150).Wait();
            await _disCache.RemoveAsync(userIdKey + ConstKey.Transaction);
            await _disCache.RemoveAsync(userIdKey + ConstKey.Bank);
            await _disCache.RemoveAsync(userIdKey + ConstKey.Session);
        }


       

    }
}
