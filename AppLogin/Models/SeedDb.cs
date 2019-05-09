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
            }

            var result = await _userHelper.CreateUserAsync(user, "123456");

            if (result !=IdentityResult.Success)
            {
                throw new InvalidOperationException("No se pudo mi hermnao " + result.ToString());
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
