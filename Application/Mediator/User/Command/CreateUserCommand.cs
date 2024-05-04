using Application.Cqrs.Commands;
using Domain.DTOs.Shared;
using System.Collections;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Application.Extensions;
using Application.Database;
using Application.Mediator.User.DTO;
using Domain.Entities.Users;

namespace Application.Mediator.User.Command
{
    public class CreateUserCommand : ICommand<ServiceRespnse>
    {
        public CreateUserDTO CreateUserDTO { get; set; }


        public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, ServiceRespnse>
        {
            private readonly IUnitOfWork _ProtectedDb;

            public CreateUserCommandHandler(IUnitOfWork protectedDb)
            {
                _ProtectedDb = protectedDb;
            }

            public async Task<ServiceRespnse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
            {
                // set validation here if you need with hash table
                var Errors = new Hashtable();

                byte[]? saltSet = null;
                byte[]? hashSet = null;
                //generate Hash for password
                HashExtension.MakeHmacHashCode(request.CreateUserDTO.Password, out var hash, out var salt);
                saltSet = salt;
                hashSet = hash;

                var userExist = false;
                var repository = _ProtectedDb.GetRepository<Domain.Entities.Users.User>();
                //get repository
                if (!string.IsNullOrEmpty(request.CreateUserDTO.Mobile))
                {
                    userExist = await repository.GetAsNoTrackingQuery()
                       .AnyAsync(z => z.Mobile == request.CreateUserDTO.Mobile, cancellationToken: cancellationToken);
                }

                if (userExist)
                {
                    Errors.Add("Mobile", "Mobile Must Be Unique");
                    return new ServiceRespnse().Failed(HttpStatusCode.NotFound, Errors);
                }

                var insertedData = await repository.AddAsync(new Domain.Entities.Users.User
                {
                    TelegramId = request.CreateUserDTO.TelegramId,
                    PasswordHash = hashSet.Length > 0 ? hashSet : null,
                    PasswordSalt = saltSet.Length > 0 ? saltSet : null,
                    Mobile = request.CreateUserDTO.Mobile,
                    Email = request.CreateUserDTO.Email,
                    FirstName = request.CreateUserDTO.FirstName,
                    FullName = request.CreateUserDTO.FullName,
                    LastName = request.CreateUserDTO.LastName,
                    UserType = UserType.DefaultUser,
                    UserName = request.CreateUserDTO.UserName,
                    RoleId = 3
                }); ;

                await _ProtectedDb.CommitAsync(cancellationToken);

                return new ServiceRespnse().OK();
            }
        }
    }
}