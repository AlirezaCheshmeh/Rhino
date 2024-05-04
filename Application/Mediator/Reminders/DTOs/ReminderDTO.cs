using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mediator.Reminders.DTOs
{
    public class ReminderDTO
    {
        public string Description { get; set; }
        public decimal Mount { get; set; }
        public DateTime RemindDate { get; set; }
        public bool IsRemindMeAgain { get; set; }
        public DateTime? RemindAgainDate { get; set; }
        public long Id { get; set; }
        public long TelegramId { get; set; }
    }
}
