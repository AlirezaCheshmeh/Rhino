using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mediator.Banks.DTOs
{
    public class BankDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string SVG { get; set; }
        public string Branch { get; set; }
        public long TelegramId { get; set; }
    }
}
