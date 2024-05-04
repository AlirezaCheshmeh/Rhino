using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mediator.Authorize.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Mobile Must Has Value")]
        public string Mobile { get; set; }
        [Required(ErrorMessage = "Password Must Has Value")]
        public string Password { get; set; }
    }
}
