using Application.Common;
using Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using Domain.DTOs.Shared;
using Domain.Entities.Transactions;

namespace Application.Mediator.Transactions.Command
{
    public class UpdateTransactionCommand : ICommand<ServiceRespnse>
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public long TelegramId { get; set; }

        public class UpdateTransactionCommandHandler : ICommandHandler<UpdateTransactionCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<Transaction> _transactionRepository;

            public UpdateTransactionCommandHandler(IGenericRepository<Domain.Entities.Transactions.Transaction> transactionRepository)
            {
                _transactionRepository = transactionRepository;
            }

            public async Task<ServiceRespnse> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
            {
                var errors = new Hashtable();
                var trans = await _transactionRepository.GetQuery().Where(z => z.Id == request.Id).FirstOrDefaultAsync();
                errors.Add("id", "can not find by this id");
                if (trans is null)
                    return new ServiceRespnse().Failed(System.Net.HttpStatusCode.NotFound, errors);

                trans.Amount = request.Amount;
                trans.Description = request.Description;
                trans.TelegramId = request.TelegramId;
                await _transactionRepository.UpdateAsync(trans, cancellationToken);
                return new ServiceRespnse().OK();
            }
        }
    }
}
