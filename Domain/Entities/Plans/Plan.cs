using Domain.Entities.BaseEntity;
using Domain.Entities.DiscountCodes;
using Domain.Entities.Users;

namespace Domain.Entities.Plans
{
    public class Plan:BaseEntity<long>
    {
        public long UserId { get; set; }
        public User User { get; set; }
        public long DiscountCodeId { get; set; }
        public DiscountCode DiscountCode { get; set; }
        public DateTime ValidDate { get; set; }
        public decimal Price { get; set; }
    }
}
