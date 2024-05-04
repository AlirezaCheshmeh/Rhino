using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Users;

namespace Application.Mediator.User.DTO
{
    public class GetUserInfoDTO
    {

        public string FullName { get; set; }
        public DateTime CreateDate { get; private set; } = DateTime.Now;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public int Age { get; set; }
        public string Mobile { get; set; }
        public string? Email { get; set; }
        public UserType UserType { get; set; }
        public string? Discord { get; set; }
        public string? FaceBook { get; set; }
        public string? Telegram { get; set; }
        public string? AvatarImage { get; set; }
        public string? About { get; set; }
    }
}
