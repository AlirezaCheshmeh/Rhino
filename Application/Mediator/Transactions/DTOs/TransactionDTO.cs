using Domain.Enums;

namespace Application.Mediator.Transactions.DTOs
{
    public class TransactionDTO
    {
        public long Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public TransactionType? Type { get; set; }
        public string? Description { get; set; }
        public long TelegramId { get; set; }
        public long MessageId { get; set; }
        public long? BankId { get; set; }
        public long? CategoryId { get; set; }
    }
}
