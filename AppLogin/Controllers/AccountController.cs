using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AppLogin.Helpers;
using AppLogin.Models.Entities;
using AppLogin.ViewModels;
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

        public AccountController(IUserHelper userHelper, IConfiguration configuration)
        {
            this._userHelper = userHelper;
            this._configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
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
                }

                ModelState.AddModelError(string.Empty, "The username is alredy registerd.");
                
            }

            return View(model);
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
    }

}