using Application.Common;
using Application.Cqrs.Queris;
using Application.Mediator.EventPays.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs.Shared;
using Domain.Entities.EventPays;

namespace Application.Mediator.EventPays.Query
{
    public class GetEventPayByIdQuery : IQuery<ServiceRespnse<EventPayDTO>>
    {
        public long Id { get; set; }

        public class GetPeriodTtransactionByIdQueryHandler : IQueryHandler<GetEventPayByIdQuery, ServiceRespnse<EventPayDTO>>
        {
            private readonly IGenericRepository<EventPay> _periodicRepo;

            public GetPeriodTtransactionByIdQueryHandler(IGenericRepository<EventPay> periodicRepo)
            {
                _periodicRepo = periodicRepo;
            }
            public async Task<ServiceRespnse<EventPayDTO>> Handle(GetEventPayByIdQuery request, CancellationToken cancellationToken)
            {
                var exist = await _periodicRepo.GetAsNoTrackingQuery().Where(z => z.Id == request.Id).Select(z => new EventPayDTO
                {
                    Amount = z.Amount,
                    Description = z.Description,
                    Id = z.Id,
                    PayDate = z.PayDate,
                    RemindMe = z.RemindMe,
                    TotalAmount = z.TotalAmount,
                    TransactionType = z.TransactionType,
                    Type = z.Type
                }).FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (exist is not null) return new ServiceRespnse<EventPayDTO>().OK(exist);
                Hashtable errors = new();
                errors.Add("Id", "Not Fpound");
                return new ServiceRespnse<EventPayDTO>().Failed(System.Net.HttpStatusCode.NotFound, errors);
            }
        }
    }
}
