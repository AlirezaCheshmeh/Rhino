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
using Domain.Entities.EventPays;

namespace Application.Mediator.EventPays.Commad
{
    public class DeleteEventPayCommand : ICommand<ServiceRespnse>
    {
        public long Id { get; set; }

        public class DeletePeriodTransactionCommandHandler : ICommandHandler<DeleteEventPayCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<EventPay> _periodicRepo;

            public DeletePeriodTransactionCommandHandler(IGenericRepository<EventPay> periodicRepo)
            {
                _periodicRepo = periodicRepo;
            }
            public async Task<ServiceRespnse> Handle(DeleteEventPayCommand request, CancellationToken cancellationToken)
            {
                var exist = await _periodicRepo.GetAsNoTrackingQuery().Where(z => z.Id == request.Id).FirstOrDefaultAsync();
                if (exist is null)
                {
                    Hashtable errors = new();
                    errors.Add("Id", "Not Fpound");
                    return new ServiceRespnse().Failed(System.Net.HttpStatusCode.NotFound, errors);
                }
                await _periodicRepo.SoftDeleteAsync(exist, cancellationToken);
                return new ServiceRespnse().OK();
            }
        }
    }
}
