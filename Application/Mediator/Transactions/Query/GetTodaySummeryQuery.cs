using Application.Common;
using Application.Cqrs.Queris;
using Application.Extensions;
using Application.Mediator.Transactions.DTOs;
using Domain.DTOs.Shared;
using Domain.Entities.Banks;
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
            private readonly IGenericRepository<Bank> _bankRepository;

            public GetTodaySummeryQueryHandler(IGenericRepository<Transaction> transactionRepository, IGenericRepository<Bank> bankRepository)
            {
                _transactionRepository = transactionRepository;
                _bankRepository = bankRepository;
            }

            public async Task<ServiceRespnse<GetTodaySummaryDTO>> Handle(GetTodaySummeryQuery request, CancellationToken cancellationToken)
            {
                GetTodaySummaryDTO? result;
                var repo = _transactionRepository.GetAsNoTrackingQuery().Where(z => z.TelegramId == request.TelegramId && z.Type == Domain.Enums.TransactionType.InBound && z.CreatedAt.Date == DateTime.Now.Date);
                if (repo is null || repo.Count() is 0)
                    result = new()
                    {
                        SumAmount = "0".ToPersianNumber(),
                        BiggestOutBound = "0".ToPersianNumber(),
                        Description = "موردی یافت نشد",
                        BankTransaction = "عنوان ندارد",
                    };
                else
                {
                    var maxAmount = await repo.Select(z => z.Amount).DefaultIfEmpty().MaxAsync();
                    result = new GetTodaySummaryDTO
                    {
                        SumAmount = (await repo.SumAsync(z => z.Amount)).ToString("N0").ToPersianNumber(),
                        BiggestOutBound = maxAmount.ToString("N0").ToPersianNumber(),
                        Description = (await repo.FirstOrDefaultAsync(z => z.Amount == maxAmount)).Description,
                        BankTransaction = (await _bankRepository.GetAsNoTrackingQuery().Where(x => x.Id == (repo.FirstOrDefault(z => z.Amount == maxAmount)).BankId).FirstOrDefaultAsync()).Name
                    };
                }
                return new ServiceRespnse<GetTodaySummaryDTO>().OK(result);
            }
        }
    }
}
