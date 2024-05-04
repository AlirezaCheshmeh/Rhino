using Application.Common;
using Application.Cqrs.Queris;
using Application.Mediator.EventPays.DTOs;
using Domain.DTOs.Shared;
using Domain.Entities.EventPays;
using Microsoft.EntityFrameworkCore;

namespace Application.Mediator.EventPays.Query
{
    public class GetAllEventPayQuery : IQuery<ServiceRespnse<List<EventPayDTO>>>
    {
        public int PageNumber { get; set; }
        public int Count { get; set; }
        public long TelegramId { get; set; }

        public class GetAllPeriodTransactionQueryHandler : IQueryHandler<GetAllEventPayQuery, ServiceRespnse<List<EventPayDTO>>>
        {
            private readonly IGenericRepository<EventPay> _periodicRepo;

            public GetAllPeriodTransactionQueryHandler(IGenericRepository<EventPay> periodicRepo)
            {
                _periodicRepo = periodicRepo;
            }
            public async Task<ServiceRespnse<List<EventPayDTO>>> Handle(GetAllEventPayQuery request, CancellationToken cancellationToken)
            {
                var repo = _periodicRepo.GetAsNoTrackingQuery().Where(z => z.TelegramId == request.TelegramId);
                var data = await repo.Select(z => new EventPayDTO
                {
                    Amount = z.Amount,
                    Description = z.Description,
                    Id = z.Id,
                    PayDate = z.PayDate,
                    RemindMe = z.RemindMe,
                    TotalAmount = z.TotalAmount,
                    TransactionType = z.TransactionType,
                    Type = z.Type
                }).Skip((request.PageNumber - 1) * request.Count).Take(request.Count).ToListAsync(cancellationToken: cancellationToken);
                var total = await repo.CountAsync(cancellationToken: cancellationToken);
                return new ServiceRespnse<List<EventPayDTO>>().OK(data, total);
            }
        }
    }
}
