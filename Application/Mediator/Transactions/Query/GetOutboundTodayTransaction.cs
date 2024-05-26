using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Cqrs.Queris;
using Application.Mediator.Transactions.DTOs;
using Domain.DTOs.Shared;

namespace Application.Mediator.Transactions.Query
{
    public class GetOutboundTodayTransaction : IQuery<ServiceRespnse<GetTodayTransactionDTO>>
    {
        public long TelegramId { get; set; }
    }

    public class Handler : IQueryHandler<GetOutboundTodayTransaction , ServiceRespnse<GetTodayTransactionDTO>>
    {
        public Handler()
        {
            
        }
        public Task<ServiceRespnse<GetTodayTransactionDTO>> Handle(GetOutboundTodayTransaction request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
