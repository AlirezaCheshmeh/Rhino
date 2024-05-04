using Application.Common;
using Application.Cqrs.Commands;
using Application.Mediator.Banks.DTOs;
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
    public class InsertBankCommand : ICommand<ServiceRespnse>
    {
        public BankDTO dto { get; set; }

        public class InsertBankCommandHandler : ICommandHandler<InsertBankCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<Bank> _bankRepository;

            public InsertBankCommandHandler(IGenericRepository<Bank> bankRepository)
            {
                _bankRepository = bankRepository;
            }

            public async Task<ServiceRespnse> Handle(InsertBankCommand request, CancellationToken cancellationToken)
            {
                var exist = await _bankRepository.GetAsNoTrackingQuery().AnyAsync(z => z.Name == request.dto.Name && z.Branch == request.dto.Branch);
                Hashtable errors = new();

                if (exist)
                {
                    errors.Add("name", "duplicate record");
                    errors.Add("branch", "duplicate record");
                    return new ServiceRespnse().Failed(System.Net.HttpStatusCode.BadRequest, errors);
                }

                await _bankRepository.AddAsync(new Bank
                {
                    Branch = request.dto.Branch,
                    Name = request.dto.Name,
                    SVG = request.dto.SVG
                }, cancellationToken);

                return new ServiceRespnse().OK();

            }
        }
    }
}
