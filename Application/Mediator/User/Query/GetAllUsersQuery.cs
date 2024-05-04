using Application.Cqrs.Queris;
using Application.Database;
using Application.Mediator.User.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs.Shared;

namespace Application.Mediator.User.Query
{
    public class GetAllUsersQuery : IQuery<IServiceResponse<IReadOnlyList<GetAllUserDTO>>>
    {
        public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, IServiceResponse<IReadOnlyList<GetAllUserDTO>>>
        {
            private readonly IUnitOfWork _unitOfWork;

            public GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
            {

                _unitOfWork = unitOfWork;
            }

            public async Task<IServiceResponse<IReadOnlyList<GetAllUserDTO>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
            {
                var repository = _unitOfWork.GetRepository<Domain.Entities.Users.User>();
                var data = await repository.GetAsNoTrackingQuery()
                    .Select(z => new GetAllUserDTO
                    {
                        Id = z.Id,
                        UserName = z.UserName,
                    }).ToListAsync(cancellationToken: cancellationToken);

                return new ServiceRespnse<IReadOnlyList<GetAllUserDTO>>().OK(data, total: null);
            }
        }
    }
}
