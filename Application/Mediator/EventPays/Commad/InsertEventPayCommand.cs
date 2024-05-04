using Application.Common;
using Application.Cqrs.Commands;
using Application.Mediator.EventPays.DTOs;
using Domain.DTOs.Shared;
using Domain.Entities.EventPays;

namespace Application.Mediator.EventPays.Commad
{
    public class InsertEventPayCommand : ICommand<ServiceRespnse>
    {
        public EventPayDTO dto { get; set; }


        public class InsertEventPayCommandCommandHandler : ICommandHandler<InsertEventPayCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<EventPay> _periodicRepo;

            public InsertEventPayCommandCommandHandler(IGenericRepository<EventPay> periodicRepo)
            {
                _periodicRepo = periodicRepo;
            }

            public async Task<ServiceRespnse> Handle(InsertEventPayCommand request, CancellationToken cancellationToken)
            {
                await _periodicRepo.AddAsync(new EventPay
                {
                    Amount = request.dto.Amount,
                    TotalAmount = request.dto.TotalAmount,
                    Description = request.dto.Description,
                    PayDate = request.dto.PayDate,
                    RemindMe = request.dto.RemindMe,
                    TransactionType = request.dto.TransactionType,
                    Type = request.dto.Type,
                    TelegramId = request.dto.TelegramId
                });
                return new ServiceRespnse().OK();
            }
        }
    }
}
