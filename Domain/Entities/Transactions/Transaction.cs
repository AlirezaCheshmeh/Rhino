using Domain.Entities.Banks;
using Domain.Entities.BaseEntity;
using Domain.Entities.Categories;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Transactions
{
    public class Transaction : BaseEntity<int>
    {
        public decimal Amount { get; set; }
        public TransactionType? Type { get; set; }
        public string? Description { get; set; }
        public long TelegramId { get; set; }
        public long MessageId { get; set; }
        public long? BankId { get; set; }
        public Bank? Bank { get; set; }
        public long? CategoryId { get; set; }
        public Category? Category { get; set; }
    }


}
