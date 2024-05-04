using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs.Shared;

namespace Application.Services.EncryptServices
{
    public interface IEncryptService
    {
        IServiceResponse<string> Encrypt(string text);
        IServiceResponse<string> Decrypt(string text);

    }
}
