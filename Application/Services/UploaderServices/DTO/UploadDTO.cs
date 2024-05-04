using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.UploaderServices.DTO
{
    public class UploadDTO
    {
        public string Path { get; set; }
        public IFormFile File { get; set; }
    }
}
