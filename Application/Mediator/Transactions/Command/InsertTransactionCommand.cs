using Application.Common;
using Application.Cqrs.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Domain.DTOs.Shared;
using Domain.Entities.Transactions;

namespace Application.Mediator.Transactions.Command
{
    public class InsertTransactionCommand : ICommand<ServiceRespnse>
    {
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public long TelegramId { get; set; }

        public class InsertTransactionCommandHandler : ICommandHandler<InsertTransactionCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<Transaction> _transactionRepository;

            public InsertTransactionCommandHandler(IGenericRepository<Transaction> transactionRepository)
            {
                _transactionRepository = transactionRepository;
            }

            public async Task<ServiceRespnse> Handle(InsertTransactionCommand request, CancellationToken cancellationToken)
            {
                await _transactionRepository.AddAsync(new Transaction
                {
                    Amount = request.Amount,
                    Description = request.Description,
                    TelegramId = request.TelegramId
                }, cancellationToken);
                return new ServiceRespnse().OK();
            }
        }
    }
}
