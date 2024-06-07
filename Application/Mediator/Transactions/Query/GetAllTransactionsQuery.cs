using Application.Common;
using Application.Cqrs.Queris;
using Application.Mediator.Transactions.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs.Shared;
using Domain.Entities.Transactions;
using Domain.Enums;
using Application.Extensions;

namespace Application.Mediator.Transactions.Query
{
    public class GetAllTransactionsQuery : IQuery<ServiceRespnse<List<TransactionDTO>>>
    {
        public int PageNumber { get; set; }
        public TransactionType? Type { get; set; }
        public int Count { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? FromPrice { get; set; }
        public long TelegramId { get; set; }
        public decimal? ToPrice { get; set; }
        public long? CategoryId { get; set; }
        public long? BankId { get; set; }

        public class GetAllTransactionsQueryHandler : IQueryHandler<GetAllTransactionsQuery, ServiceRespnse<List<TransactionDTO>>>
        {
            private readonly IGenericRepository<Transaction> _transactionRepository;

            public GetAllTransactionsQueryHandler(IGenericRepository<Transaction> transactionRepository)
            {
                _transactionRepository = transactionRepository;
            }

            public async Task<ServiceRespnse<List<TransactionDTO>>> Handle(GetAllTransactionsQuery request, CancellationToken cancellationToken)
            {
                var Repo = _transactionRepository.GetQuery().Where(z => z.TelegramId == request.TelegramId);

                //filters
                if (request.CategoryId.HasValue)
                    Repo = Repo.Where(z => z.CategoryId == request.CategoryId.Value);
                if (request.BankId.HasValue)
                    Repo = Repo.Where(z => z.BankId == request.BankId.Value);
                if (request.FromPrice.HasValue)
                    Repo = Repo.Where(z => z.Amount > request.FromPrice.Value);
                if (request.ToPrice.HasValue)
                    Repo = Repo.Where(z => z.Amount > request.ToPrice.Value);
                if (request.FromDate.HasValue)
                    Repo = Repo.Where(z => z.CreatedAt.Date >= request.FromDate.Value.Date);
                if (request.ToDate.HasValue)
                    Repo = Repo.Where(z => z.CreatedAt.Date <= request.ToDate.Value.Date);
                if (request.Type.HasValue)
                    Repo = Repo.Where(z => z.Type == request.Type.Value);

                var trans = await Repo.Select(x => new TransactionDTO
                {
                    Amount = x.Amount,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt,
                    BankId = x.BankId,
                    CategoryId = x.CategoryId,
                    Id = x.Id,
                    TelegramId = x.TelegramId,
                    Type = x.Type
                }).Skip((request.PageNumber - 1) * request.Count)
                .Take(request.Count).ToListAsync();
                var totalCount = await Repo.CountAsync();
                var totalAmount = await Repo.SumAsync(z => z.Amount);
                string totalResAmount = totalAmount.ToString("N0").ToPersianNumber();
                return new ServiceRespnse<List<TransactionDTO>>().OK(trans, total: totalCount, totalAmount: totalResAmount);
            }
        }
    }
}
