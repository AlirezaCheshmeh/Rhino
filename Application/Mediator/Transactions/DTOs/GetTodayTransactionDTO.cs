using Domain.Entities.Banks;
using Domain.Entities.Categories;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mediator.Transactions.DTOs
{
    public class GetTodayTransactionDTO
    {
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string CategoryName { get; set; }
        public string BankName { get; set; }
    }
}
