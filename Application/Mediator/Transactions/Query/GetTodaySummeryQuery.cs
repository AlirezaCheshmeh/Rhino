using Application.Common;
using Application.Cqrs.Queris;
using Application.Mediator.Transactions.DTOs;
using Domain.DTOs.Shared;
using Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Application.Mediator.Transactions.Query
{
    public class GetTodaySummeryQuery : IQuery<ServiceRespnse<GetTodaySummaryDTO>>
    {
        public long TelegramId { get; set; }
        public DateTime? Date { get; set; }
        public class GetTodaySummeryQueryHandler : IQueryHandler<GetTodaySummeryQuery, ServiceRespnse<GetTodaySummaryDTO>>
        {
            private readonly IGenericRepository<Transaction> _transactionRepository;

            public GetTodaySummeryQueryHandler(IGenericRepository<Transaction> transactionRepository)
            {
                _transactionRepository = transactionRepository;
            }

            public async Task<ServiceRespnse<GetTodaySummaryDTO>> Handle(GetTodaySummeryQuery request, CancellationToken cancellationToken)
            {
                var repo = _transactionRepository.GetAsNoTrackingQuery().Where(z => z.TelegramId == request.TelegramId && z.Type == Domain.Enums.TransactionType.OutBound && (request.Date.HasValue ? z.CreatedAt == request.Date.Value : z.CreatedAt == DateTime.Now));
                var maxAmount = repo.Select(z => z.Amount).Max();
                var result = new GetTodaySummaryDTO
                {
                    SumAmount = (await repo.SumAsync(z => z.Amount)).ToString("N0"),
                    BiggestOutBound = maxAmount.ToString("N0"),
                    Description = (await repo.FirstOrDefaultAsync(z => z.Amount == maxAmount)).Description
                };
                return new ServiceRespnse<GetTodaySummaryDTO>().OK(result);
            }
        }
    }
}
