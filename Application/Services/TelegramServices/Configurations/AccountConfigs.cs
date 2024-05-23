using Application.Common;
using Domain.Entities.UserPurchases;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.TelegramServices.Configurations
{
    public class AccountConfigs
    {

        private readonly IGenericRepository<UserPurchase> _perchaseRepository;

        public AccountConfigs(IGenericRepository<UserPurchase> perchaseRepository)
        {
            _perchaseRepository = perchaseRepository;
        }

        public async Task<bool> CheckUserActiveAccount(long telegramId)
        {
            var userPerchaseExist = await _perchaseRepository.GetAsNoTrackingQuery().OrderByDescending(z=>z.Id).AnyAsync(z => z.TelegramId == telegramId && z.ValidDate.Date > DateTime.Now.Date);
            return userPerchaseExist ? true : false;
        }
    }
}
