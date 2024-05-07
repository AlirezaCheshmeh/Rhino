using Domain.Entities.BaseEntity;
using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.DiscountCodes
{
    public class DiscountCode:BaseEntity<long>
    {
        public string Code { get; set; }
        public DateTime ExpireDate { get; set; }
        public int UseCount { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
    }
}
