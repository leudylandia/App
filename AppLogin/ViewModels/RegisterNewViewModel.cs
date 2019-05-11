using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppLogin.ViewModels
{
    public class RegisterNewViewModel
    {
        [Required]
        [Display(Name ="First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [MinLength(6)]
        [Compare("Password")]
        public string Confirm { get; set; }
    }
}
