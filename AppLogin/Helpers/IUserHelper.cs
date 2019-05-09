using AppLogin.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppLogin.Helpers
{
    public interface IUserHelper
    {
        Task<User> FindUserByEmailAsync(string email);
        Task<IdentityResult> CreateUserAsync(User user, string password);
    }
}
