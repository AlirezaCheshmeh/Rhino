using Application.Common;
using Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Domain.DTOs.Shared;
using Domain.Entities.Banks;

namespace Application.Mediator.Banks.Command
{
    public class DeleteBankCommand : ICommand<ServiceRespnse>
    {
        public long Id { get; set; }

        public class DeleteBankCommandHandler : ICommandHandler<DeleteBankCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<Bank> _bankRepository;

            public DeleteBankCommandHandler(IGenericRepository<Bank> bankRepository)
            {
                _bankRepository = bankRepository;
            }

            public async Task<ServiceRespnse> Handle(DeleteBankCommand request, CancellationToken cancellationToken)
            {
                var bank = await _bankRepository.GetQuery().Where(x => x.Id == request.Id).FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (bank is null)
                {
                    Hashtable errors = new();
                    errors.Add("Id", "not found");
                    return new ServiceRespnse().Failed(System.Net.HttpStatusCode.NotFound, errors);
                }
                await _bankRepository.SoftDeleteAsync(bank, cancellationToken);

                return new ServiceRespnse().OK();
            }
        }
    }
}
