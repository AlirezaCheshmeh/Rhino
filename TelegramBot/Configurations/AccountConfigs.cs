using Domain.Entities.UserPurchases;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBot.Configurations
{
    public class AccountConfigs
    {
        public async Task<bool> CheckUserActiveAccount(long telegramId)
        {
            using (ApplicationDataContext context = new())
            {
                var userPerchaseExist = await context.Set<UserPurchase>().AnyAsync(z => z.TelegramId == telegramId && z.ValidDate.Date > DateTime.Now.Date);
                if (userPerchaseExist)
                    return true;
                else
                    return false;

            }

        }
    }
}
