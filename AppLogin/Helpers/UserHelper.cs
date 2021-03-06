﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLogin.Models.Entities;
using AppLogin.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppLogin.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserHelper(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
        }

        //Añade un role al usuario, le pasamos  el usuario y el role
        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldpassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldpassword, newPassword);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            //Verifica si el roleName existe de lo contrario lo creamos
            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                });
            }
        }

        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password); 
        }

        public async Task<User> FindUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user;
        }

        //Verifica si el usuario pertenece a ese rol
        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<SignInResult> ValidatePasswordAsync(User user, string password)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, false);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }

        public async Task<List<User>> GetAllUserAsync()
        {
            return await _userManager.Users
                .OrderBy(u => u.Name)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        public async Task RemoveUserFromRoleAsync(User user, string roleName)
        {
            await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        public async Task DeleteUserAsync(User user)
        {
            await _userManager.DeleteAsync(user);
        }
    }
}
