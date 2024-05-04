using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mediator.Transactions.DTOs
{
    public class GetTodaySummaryDTO
    {
        public string BiggestOutBound { get; set; }
        public string Description { get; set; }
        public string SumAmount { get; set; }
    }
}
