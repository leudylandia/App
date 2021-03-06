﻿using AppLogin.Models.Entities;
using AppLogin.ViewModels;
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
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        Task<IdentityResult> UpdateUserAsync(User user);
        Task<IdentityResult> ChangePasswordAsync(User user, string oldpassword, string newPassword);
        Task<SignInResult> ValidatePasswordAsync(User user, string password); //No loguea, solo dice si es valido para loguearse
        Task CheckRoleAsync(string roleName);
        Task AddUserToRoleAsync(User user, string roleName);
        Task<bool> IsUserInRoleAsync(User user, string roleName);

        Task<string> GenerateEmailConfirmationTokenAsync(User user);
        Task<IdentityResult> ConfirmEmailAsync(User user, string token);
        Task<User> GetUserByIdAsync(string userId);

        Task<string> GeneratePasswordResetTokenAsync(User user);
        Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);
        Task<List<User>> GetAllUserAsync();
        Task RemoveUserFromRoleAsync(User user, string role);
        Task DeleteUserAsync(User user);
    }
}
