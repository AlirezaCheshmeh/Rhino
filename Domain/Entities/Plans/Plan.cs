using Domain.Entities.BaseEntity;
using Domain.Entities.DiscountCodes;
using Domain.Entities.UserPurchases;
using Domain.Entities.Users;

namespace Domain.Entities.Plans
{
    public class Plan:BaseEntity<long>
    {
        public string Title { get; private set; }
        public ICollection<DiscountCode> DiscountCodes { get; private set; }
        public ICollection<UserPurchase> UserPurchases { get; private set; }
        public decimal Price { get; private set; }

        public void SetTitle(string titel) => Title = titel;
        public void SetPrice(decimal price) => Price = price;

        //factory methodes
        public Plan CreatePlan(decimal price, string title) => new()
        {
            Title = title,
            Price = price
        };
    }
}
