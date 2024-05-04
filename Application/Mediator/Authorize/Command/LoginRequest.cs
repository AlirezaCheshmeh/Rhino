using Application.Cqrs.Commands;
using Application.Database;
using Application.Extensions;
using Application.Mediator.Authorize.DTO;
using Application.Services.AuthorizeServices;
using Application.Services.AuthorizeServices.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs.Shared;

namespace Application.Mediator.Authorize.Command
{
    public class LoginRequest : ICommand<IServiceResponse<TokenDTO>>
    {
        public LoginDTO LoginDTO { get; set; }

        public class LoginRequestHandler : ICommandHandler<LoginRequest, IServiceResponse<TokenDTO>>
        {
            private readonly IToken _token;
            private readonly IUnitOfWork _unitOfWork;

            public LoginRequestHandler(IToken token, IUnitOfWork unitOfWork)
            {
                _token = token;
                _unitOfWork = unitOfWork;
            }
            public async Task<IServiceResponse<TokenDTO>> Handle(LoginRequest request, CancellationToken cancellationToken)
            {
                //set validation
                var validaion = new Hashtable();
                //set operator
                var Operator = new ServiceRespnse<TokenDTO>();
                //get user repository 
                var dto = request.LoginDTO;
                var repository = _unitOfWork.GetRepository<Domain.Entities.Users.User>();
                var userExist = await repository.GetAsNoTrackingQuery().FirstOrDefaultAsync(x => x.Mobile == dto.Mobile, cancellationToken: cancellationToken);
                if (userExist == null)//check user exist
                {
                    Operator.Message = "There Is No User By This Mobile";
                    Operator.IsSuccess = false;
                }
                else
                {
                    var verify = HashExtension.VerifyHashPassword(dto.Password, userExist.PasswordSalt, userExist.PasswordHash);
                    if (verify && userExist.IsActive == true)
                    {
                        //generate token
                        var token = await _token.Get(userExist);
                        token.FullName = userExist.FullName;
                        Operator.Message = "Operation success";
                        Operator.StatusCode = System.Net.HttpStatusCode.OK;
                        Operator.IsSuccess = true;
                        Operator.Data = token;
                    }
                    else if (userExist.IsActive == false)
                    {
                        Operator.Message = "User Is Not Active";
                        Operator.IsSuccess = false;
                        Operator.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    }
                    else
                    {
                        Operator.Message = "Invalid Pass Or Mobile";
                        Operator.IsSuccess = false;
                        Operator.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    }
                }

                return Operator;
            }
        }
    }
}
