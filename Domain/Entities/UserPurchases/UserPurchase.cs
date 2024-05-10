using Domain.Entities.BaseEntity;
using Domain.Entities.DiscountCodes;
using Domain.Entities.Plans;
using Domain.Entities.Users;

namespace Domain.Entities.UserPurchases
{
    public class UserPurchase : BaseEntity<long>
    {
        public DateTime ValidDate { get; private set; }
        public long PlanId { get; private set; }
        public Plan Plan { get; private set; }
        public long? DisCountCodeId { get; private set; }
        public DiscountCode DiscountCode { get; private set; }
        public decimal? OffPrice { get; private set; }
        public decimal CustomerProfit { get; private set; }
        public decimal PayedPrice { get; private set; }
        public decimal PlanBasePrice { get; private set; }
        public long TelegramId { get; private set; }

        public void SetValiddate(DateTime date) => ValidDate = date;
        public void SetDiscountCodeId(long codeId) => DisCountCodeId = codeId;
        public void SetOffPrice(decimal price) => OffPrice = price;
        public void SetBasePlanPrice(decimal price) => PlanBasePrice = price;
        public void SetPlanId(long id) => PlanId = id;
        public void SetPayedPrice(decimal basePrice,decimal offPrice) => PayedPrice = basePrice - offPrice;


        //factory methodes use these methodes like constructor
        public UserPurchase CreateUserPurchase(DateTime validDate, long telegramId,
            long planId, decimal planBasePrice,
            decimal? offPrice, long? disCountId) => new()
            {
                PayedPrice = offPrice.HasValue?(planBasePrice - offPrice.Value):0,
                CustomerProfit = offPrice.HasValue?(planBasePrice - (PayedPrice - offPrice.Value)):0,
                OffPrice = offPrice,
                DisCountCodeId = disCountId,
                PlanBasePrice = planBasePrice,
                PlanId = planId,
                TelegramId = telegramId,
                ValidDate = validDate,
            };




    }
}
