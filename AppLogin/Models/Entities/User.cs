//using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AppLogin.Models.Entities
{
    public class User : Microsoft.AspNetCore.Identity.IdentityUser
    {
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        [NotMapped] //Para no guardar el campo en la db, 
        [Display(Name ="Is Admin?")]
        public bool IsAdmin { get; set; }
    }
}
