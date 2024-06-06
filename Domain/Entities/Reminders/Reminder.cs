using Domain.Entities.BaseEntity;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Reminders
{
    public class Reminder : BaseEntity<long>
    {
        public string Description { get; set; }
        public long ChatId { get; set; }
        public ReminderType Type { get; set; }
        public bool IsExpire { get; set; }
        public decimal Amount { get; set; }
        public DateTime RemindDate { get; set; }
        public bool IsRemindMeAgain { get; set; }
        public DateTime? RemindAgainDate { get; set; }
        public long TelegramId { get; set; }
    }
}
