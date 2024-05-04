using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.Mediator.EventPays.DTOs
{
    public class EventPayDTO
    {
        public long Id { get; set; }
        public DateTime PayDate { get; set; }
        public long TelegramId { get; set; }
        public decimal Amount { get; set; }
        public bool RemindMe { get; set; }
        public decimal TotalAmount { get; set; }
        public PeriodTransactionType Type { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Description { get; set; }
    }
}
