using Application.Common;
using Application.Cqrs.Commands;
using Application.Mediator.EventPays.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Windows.Input;
using Domain.DTOs.Shared;
using Domain.Entities.EventPays;

namespace Application.Mediator.EventPays.Commad
{
    public class UpdateEventPayCommand : ICommand<ServiceRespnse>
    {
        public EventPayDTO dto { get; set; }


        public class UpdateEventPayCommandCommandHadnler : ICommandHandler<UpdateEventPayCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<EventPay> _periodicRepo;

            public UpdateEventPayCommandCommandHadnler(IGenericRepository<EventPay> periodicRepo)
            {
                _periodicRepo = periodicRepo;
            }

            public async Task<ServiceRespnse> Handle(UpdateEventPayCommand request, CancellationToken cancellationToken)
            {
                var exist = await _periodicRepo.GetAsNoTrackingQuery().AnyAsync(z => z.Id == request.dto.Id);
                if (!exist)
                {
                    Hashtable errors = new();
                    errors.Add("Id", "Not Fpound");
                    return new ServiceRespnse().Failed(System.Net.HttpStatusCode.NotFound, errors);
                }

                await _periodicRepo.AddAsync(new EventPay
                {
                    Id = request.dto.Id,
                    Amount = request.dto.Amount,
                    TotalAmount = request.dto.TotalAmount,
                    Description = request.dto.Description,
                    PayDate = request.dto.PayDate,
                    RemindMe = request.dto.RemindMe,
                });
                return new ServiceRespnse().OK();
            }
        }
    }
}
