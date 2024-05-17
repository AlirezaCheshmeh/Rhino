using Application.Common;
using Application.Cqrs.Commands;
using Domain.DTOs.Shared;
using Domain.Entities.Transactions;
using Application.Mediator.Transactions.DTOs;
using AutoMapper;

namespace Application.Mediator.Transactions.Command
{
    public class InsertTransactionCommand : ICommand<ServiceRespnse>
    {
        public TransactionDTO dto { get; set; }

        public class InsertTransactionCommandHandler : ICommandHandler<InsertTransactionCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<Transaction> _transactionRepository;
            private readonly IMapper _mapper;

            public InsertTransactionCommandHandler(IGenericRepository<Transaction> transactionRepository, IMapper mapper)
            {
                _transactionRepository = transactionRepository;
                _mapper = mapper;
            }

            public async Task<ServiceRespnse> Handle(InsertTransactionCommand request, CancellationToken cancellationToken)
            {
                var transaction = _mapper.Map<Transaction>(request.dto);
                await _transactionRepository.AddAsync(transaction, cancellationToken);
                return new ServiceRespnse().OK();
            }
        }
    }
}
