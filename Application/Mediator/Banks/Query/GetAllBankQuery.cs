using Application.Common;
using Application.Cqrs.Queris;
using Application.Mediator.Banks.DTOs;
using Domain.DTOs.Shared;
using Domain.Entities.Banks;
using Microsoft.EntityFrameworkCore;

namespace Application.Mediator.Banks.Query
{
    public class GetAllBankQuery : IQuery<ServiceRespnse<List<BankDTO>>>
    {
        public int PageNumber { get; set; }
        public int Count { get; set; }

        public class GetAllBankQueryHandler : IQueryHandler<GetAllBankQuery, ServiceRespnse<List<BankDTO>>>
        {
            private readonly IGenericRepository<Bank> _bankRepository;

            public GetAllBankQueryHandler(IGenericRepository<Bank> bankRepository)
            {
                _bankRepository = bankRepository;
            }

            public async Task<ServiceRespnse<List<BankDTO>>> Handle(GetAllBankQuery request, CancellationToken cancellationToken)
            {
                var repo = _bankRepository.GetAsNoTrackingQuery();
                //TODO:set filters here

                var banks = await repo.Select(z => new BankDTO
                {
                    Branch = z.Branch,
                    Id = z.Id,
                    Name = z.Name,
                    SVG = z.SVG
                }).Skip((request.PageNumber - 1) * request.Count).Take(request.Count).ToListAsync(cancellationToken: cancellationToken);
                var totalCounts = await repo.CountAsync(cancellationToken: cancellationToken);
                return new ServiceRespnse<List<BankDTO>>().OK(banks, total: totalCounts);
            }
        }
    }
}
