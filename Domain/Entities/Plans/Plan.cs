using Domain.Entities.BaseEntity;
using Domain.Entities.DiscountCodes;
using Domain.Entities.Users;

namespace Domain.Entities.Plans
{
    public class Plan:BaseEntity<long>
    {
        public long UserId { get; private set; }
        public string Title { get; private set; }
        public User User { get; private set; }
        public ICollection<DiscountCode> DiscountCode { get; private set; }
        public decimal Price { get; private set; }

        public void SetTitle(string titel) => Title = titel;
        public void SetPrice(decimal price) => Price = price;

        //factory methodes
        public Plan CreatePlan(long userId, decimal price, string title) => new()
        {
            Title = title,
            UserId =userId,
            Price = price
        };
    }
}
