using Microsoft.Extensions.Caching.Distributed;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Application.Services.TelegramServices.BaseMethods.HandleUpdate;
using static Application.Services.TelegramServices.ConstVariable.ConstCallBackData;
using Application.Services.TelegramServices.ConstVariable;

namespace Application.Services.TelegramServices.Configurations
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
                                     .WithCallbackData("🆕 ایجاد پرداختی جدید", Menu.OutboundTransaction),
                                  InlineKeyboardButton
                                    .WithCallbackData("🗞 ایجاد دریافتی جدید", Menu.InboundTransaction),
                            },
                            //new[]
                            //{
                               
                            //},
                            new[]
                            {
                                InlineKeyboardButton
                                    .WithCallbackData("🗓 یادآوری رویداد دوره‌ای", Menu.PeriodicReminder),

                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("⚙ تنظیمات",Menu.Settings),
                                InlineKeyboardButton.WithCallbackData("📈 گزارشات",Menu.Reports),

                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("⁉️ راهنما", Menu.Guide),
                                InlineKeyboardButton.WithCallbackData("💬 پشتیبانی",Menu.Supporter),

                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("💳 خرید اشتراک", Menu.BuyAccount),
                            }
                        });
            await _client.SendTextMessageAsync(
            chatId: chatId,
            text: ConstMessage.Menu,
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboards);
        }


        public async Task RollBackToMenu(long userIdKey, long chatId, UserSession session)
        {
            session.MessageIds.RemoveAll(i => i == session.MessageIds.First());
            session.MessageIds.ForEach(item =>
            {
                _client.DeleteMessageAsync(chatId, item);
            });
            Task.Delay(50).Wait();
            await _disCache.RemoveAsync(userIdKey + ConstKey.Transaction);
            await _disCache.RemoveAsync(userIdKey + ConstKey.Bank);
            await _disCache.RemoveAsync(userIdKey + ConstKey.Session);
            await _disCache.RemoveAsync(userIdKey + ConstKey.InBoundMonth);
            await _disCache.RemoveAsync(userIdKey + ConstKey.OutBoundMonth);
            await _disCache.RemoveAsync(userIdKey + ConstKey.ReminderMessageId);
        }




    }
}
