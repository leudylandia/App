using AppLogin.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppLogin.Models
{
    public class LoginDbContext: IdentityDbContext<User>
    {
        public LoginDbContext(DbContextOptions<LoginDbContext> options) : base(options)
        {

        }

        public DbSet<Game> Games { get; set; }
    }
}
