using Application.Common;
using Application.Cqrs.Queris;
using Application.Mediator.Transactions.DTOs;
using Domain.DTOs.Shared;
using Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace Application.Mediator.Transactions.Query
{
    public class GetTransactionByIdQuery : IQuery<ServiceRespnse<TransactionDTO>>
    {
        public long Id { get; set; }

        public class GetTransactionByIdQueryHandler : IQueryHandler<GetTransactionByIdQuery, ServiceRespnse<TransactionDTO>>
        {
            private readonly IGenericRepository<Transaction> _transactionRepository;

            public GetTransactionByIdQueryHandler(IGenericRepository<Transaction> transactionRepository)
            {
                _transactionRepository = transactionRepository;
            }

            public async Task<ServiceRespnse<TransactionDTO>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
            {
                var errors = new Hashtable();
                var trans = await _transactionRepository.GetQuery().Where(z => z.Id == request.Id).Select(x => new TransactionDTO
                {
                    Amount = x.Amount,
                    CategoryId = x.CategoryId,
                    BankId = x.BankId,
                    Description = x.Description,
                    Id = request.Id,
                    TelegramId = x.TelegramId,
                    Type = x.Type,
                    CreatedAt = x.CreatedAt
                }).FirstOrDefaultAsync();
                errors.Add("id", "can not find by this id");
                if (trans is null)
                    return new ServiceRespnse<TransactionDTO>().Failed(System.Net.HttpStatusCode.NotFound, errors);
                return new ServiceRespnse<TransactionDTO>().OK(trans);

            }
        }
    }
}
