﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AppLogin.Helpers;
using AppLogin.Models.Entities;
using AppLogin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AppLogin.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConfiguration _configuration; //Para acceder a los valores que hay en el appsetting.json, vamos acceder  a los token
        private readonly IMailHelper _mailHelper;

        public AccountController(IUserHelper userHelper, IConfiguration configuration, IMailHelper mailHelper)
        {
            this._userHelper = userHelper;
            this._configuration = configuration;
            this._mailHelper = mailHelper;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
             var users = await this._userHelper.GetAllUserAsync();

           //Lamda users.ForEach(async u => u.IsAdmin = await _userHelper.IsUserInRoleAsync(u, "Admin"));

            foreach (var item in users)
            {
                var myUser = await _userHelper.GetUserByIdAsync(item.Id);
                if (myUser != null)
                    item.IsAdmin = await _userHelper.IsUserInRoleAsync(myUser, "Admin");
            }

            return View(users);
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(HomeController.Index), "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if(ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);

                if(result.Succeeded)
                {
                    //Esto es por si hay una Url
                    if (Request.Query.Keys.Contains("ReturnUrl"))
                        return Redirect(Request.Query["ReturnUrl"].First());

                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Failed to login");
            return View(model);
            
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterNewViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Buscarmos el email para ver si ya existe algun usuario con ese correo
                var user = await _userHelper.FindUserByEmailAsync(model.Username);

                if(user == null) //Verificamos si no encontro un usuario con ese correo para seguir 
                {
                    //TODO:
                    user = new User
                    {
                        Email = model.Username,
                        UserName = model.Username,
                        Name = model.FirstName,
                        LastName = model.LastName
                    };

                    //Creamos el usuario 
                    var result = await _userHelper.CreateUserAsync(user, model.Password);

                    //Verificamos que se haya creado el usuario, de lo contrario mandamos un mensaje a la vista
                    if (result != IdentityResult.Success)
                    {
                        this.ModelState.AddModelError(string.Empty, "Could not be created.");
                        return View(model);
                    }

                    //Ante  logeabamos el usuario, ahora debe confirmar via email
                    var myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    var tokenLink = Url.Action("ConfirmEmail", "Account", new
                    {
                        userid = user.Id,
                        token = myToken
                    }, protocol: HttpContext.Request.Scheme);

                    var msj = $"<h1>Email Confirmation</h1> To allow the user please click in this link:</br></br><a href={tokenLink}>Confirm Email </a>";

                    _mailHelper.SendMail(model.Username, "Email Confirmation", msj);
                    ViewBag.Message = "The instuction to allow you user has been sent to email.";
                    return View(model);

                    #region Loguear desde que se registre
                    /* //Aqui inicia par loguear el usuario lo puse en comentario porque ahora estamos usando confirmacion de correo, no estamos logueando de una vez que se regustre
                    //Luego de que el usuario es creado procedemos a iniciar session, pasamos los datos a loginViewModel para enviarselo al metdo login
                    var loginVM = new LoginViewModel
                    {
                        Username = model.Username,
                        Password = model.Password,
                        RememberMe = false
                    };

                    //Enviamos los datos al metodo  login para iniciar session
                    var result2 = await _userHelper.LoginAsync(loginVM);

                    //Si los datos son correcto, ya logueado nos vamos para la pantalla principal
                    if(result2.Succeeded)
                    {
                        return RedirectToAction(nameof(HomeController.Index), "Home");
                    }

                    //Es imposible de que llegue a esta parte, ya que si el usuario fue creado con exito, lo va a loggear
                    this.ModelState.AddModelError(string.Empty, "Could not be login.");
                    return View(model);
                    */
                    #endregion
                }

                ModelState.AddModelError(string.Empty, "The username is alredy registerd.");
                
            }

            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return NotFound();

            var user = await _userHelper.GetUserByIdAsync(userId); //Validamos el usuario
            if (user == null)
                return NotFound();

            var result = await _userHelper.ConfirmEmailAsync(user, token); //Ejecutamos el metodo de confirmacion del email
            if (!result.Succeeded)
                return NotFound();

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction(nameof(AccountController.Index), "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ChangeUser()
        {
            var user = await _userHelper.FindUserByEmailAsync(this.User.Identity.Name); //Buscamos el  usuario
            var model = new ChangeUserViewModel();

            if (user != null)
            {
                model.FirstName = user.Name;
                model.LastName = user.LastName;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUser(ChangeUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.FindUserByEmailAsync(this.User.Identity.Name); //Buscamos el  usuario

                if (user != null)
                {
                    user.Name = model.FirstName;
                    user.LastName = model.LastName;
                    var response = await _userHelper.UpdateUserAsync(user);

                    if (response.Succeeded)
                        ViewBag.UserMessage = "User updated!";
                    else
                        ModelState.AddModelError(string.Empty, response.Errors.FirstOrDefault().Description); //Puedes dar varios errores, estamos seleccionando el primero jsjs
                }
            }
            else
                ModelState.AddModelError(string.Empty, "User no found");

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.FindUserByEmailAsync(this.User.Identity.Name);

                if (user != null)
                {
                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                        return RedirectToAction("ChangeUser");
                    else
                        ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault().Description);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Buscamos para saber si ese usuario existe
                var user = await _userHelper.FindUserByEmailAsync(model.Username);

                if (user != null) //Si es null, no existe y lanzamos un badrequest
                {
                    var result = await _userHelper.ValidatePasswordAsync(user, model.Password); //Validamos que  usuario y password sea corecto

                    if (result.Succeeded)
                    {
                        //Generamos los claim con el email del usuario y un Guid, cada llamado es unico
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        //Generamos la llave con la clave simestrica, con el key que pusimos en el appsetting
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration["Tokens:Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); //Generamos la credenciales
                        //Generamos el token
                        var token = new JwtSecurityToken(_configuration["Tokens:Issuer"], 
                            _configuration["Tokens:Audience"], 
                            claims, expires: DateTime.UtcNow.AddDays(15), //Valido por 15 dias
                            signingCredentials: credentials);

                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        };

                        return Created(string.Empty, results);
                    }
                }
            }

            return BadRequest();
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }

        public IActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.FindUserByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email does not correspont to a registered user.");
                    return View(model);
                }

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                var link = Url.Action("ResetPassword", "Account", new { Token = myToken }, protocol: HttpContext.Request.Scheme);
                var mailSender = new MailHelper(_configuration);
                var msj = $"<h1>Password Reset</h1> To reset the password click on link below :</br></br><a href={link}>Reset Password</a>";
                mailSender.SendMail(model.Email, "Password Reset", msj);
                ViewBag.Message = "The instructions to recover  your password has been sent to email";
                return View();
            }

            return View(model);
        }

        public IActionResult ResetPassword(string token)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userHelper.FindUserByEmailAsync(model.UserName);
            if (user != null)
            {
                var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    ViewBag.Message = "Password reset succssful.";
                    return View();
                }

                ViewBag.Message = "Error while reseting password.";
                return View(model);
            }

            ViewBag.Message = "User not found.";
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminOff(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            await _userHelper.RemoveUserFromRoleAsync(user, "Admin");
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminOn(string id)
        {
            //TODO: Revisar que no presenta los admin y no  lo quita
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            await _userHelper.AddUserToRoleAsync(user, "Admin");
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            await _userHelper.DeleteUserAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}