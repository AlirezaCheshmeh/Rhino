using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mediator.User.DTO
{
    public class CreateUserDTO
    {
        public long TelegramId { get; set; }
        [Required(ErrorMessage = "FullName Must Has Value")]
        [MaxLength(30, ErrorMessage = "More Than 30 chars")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Password Must Has Value")]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Pass And RePass Is Not Equals")]
        [Required(ErrorMessage = "RePassword Must Has Value")]
        public string RePassword { get; set; }
        public DateTime CreateDate { get; private set; } = DateTime.Now;
        [Required(ErrorMessage = "FirstName Must Has Value")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "LastName Must Has Value")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "UserName Must Has Value")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Age Must Has Value")]
        public int Age { get; set; }
        [Required(ErrorMessage = "Mobile Must Has Value")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Phone Number Is Invalid")]
        public string Mobile { get; set; }
        [DataType(DataType.EmailAddress, ErrorMessage = "Email Is Invalid")]
        public string? Email { get; set; }
        public string? Discord { get; set; }
        public string? FaceBook { get; set; }
        public string? Telegram { get; set; }
        public string? AvatarImage { get; set; }
        public string? About { get; set; }
        [Required(ErrorMessage = "IsReadRules Must Has Value")]
        public bool IsReadRules { get; set; }
    }
}
