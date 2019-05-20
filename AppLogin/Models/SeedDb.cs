using AppLogin.Helpers;
using AppLogin.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppLogin.Models
{
    public class SeedDb
    {
        private readonly IUserHelper _userHelper;
        private readonly LoginDbContext context;
       // private readonly UserManager<User> _userManager;

        public SeedDb(LoginDbContext context, IUserHelper userHelper)
        {
            this.context = context;
            this._userHelper = userHelper;
            //this._userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await context.Database.EnsureCreatedAsync();

            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("Customer");

            var user = await this._userHelper.FindUserByEmailAsync("leudylandia26@hotmail.com");

           if(user == null)
           {
                user = new User
                {
                    Name = "Leudy David",
                    LastName = "De los Santos S.",
                    Email = "leudylandia26@hotmail.com",
                    UserName = "leudylandia26@hotmail.com"
                };

                var result = await _userHelper.CreateUserAsync(user, "123456");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("No se pudo mi hermnao " + result.ToString());
                }

                await _userHelper.AddUserToRoleAsync(user, "Admin");
                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);
           }

            //Si el usuario ya est acreado verificamos si pertenece algun rol
            var isInrole = await _userHelper.IsUserInRoleAsync(user, "Admin");
            if (!isInrole)
            {
                await _userHelper.AddUserToRoleAsync(user, "Admin");
            }
        }

        public async Task SeedGamesAsync()
        {
            await context.Database.EnsureCreatedAsync();

            var games = new Game
            {
                Name = "Pubg Mobile",
                Console = "Xbox",
                Gender = "Action",
                User = null
            };

            context.Games.Add(games);
           // await context.SaveChangesAsync();
        }

        public async Task SeedUserAsync()
        {
            await context.Database.EnsureCreatedAsync();

            var user = await _userHelper.FindUserByEmailAsync("leudylandia26@hotmail.com");

            if (user == null)
            {
                user = new User
                {
                    Name = "Leudy David",
                    LastName = "De los Santos S.",
                    Email = "leudylandia26@hotmail.com",
                    UserName = "leudylandia26@hotmail.com"
                };
            }

            var result = await _userHelper.CreateUserAsync(user, "123456");

            if (result != IdentityResult.Success)
            {
                throw new InvalidOperationException(result.ToString());
            }
        }
    }
}
