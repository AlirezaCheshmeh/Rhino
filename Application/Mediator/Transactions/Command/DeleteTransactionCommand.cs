using Application.Common;
using Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using Domain.DTOs.Shared;
using Domain.Entities.Transactions;

namespace Application.Mediator.Transactions.Command
{
    public class DeleteTransactionCommand : ICommand<ServiceRespnse>
    {
        public long Id { get; set; }

        public class DeleteTransactionCommandHandler : ICommandHandler<DeleteTransactionCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<Transaction> _transactionRepository;

            public DeleteTransactionCommandHandler(IGenericRepository<Domain.Entities.Transactions.Transaction> transactionRepository)
            {
                _transactionRepository = transactionRepository;
            }

            public async Task<ServiceRespnse> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
            {
                var errors = new Hashtable();
                var trans = await _transactionRepository.GetQuery().Where(z => z.Id == request.Id).FirstOrDefaultAsync();
                errors.Add("id", "can not find by this id");
                if (trans is null)
                    return new ServiceRespnse().Failed(System.Net.HttpStatusCode.NotFound, errors);
                await _transactionRepository.SoftDeleteAsync(trans, cancellationToken);
                return new ServiceRespnse().OK();
            }
        }
    }
}
