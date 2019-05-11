using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLogin.Helpers;
using AppLogin.Models.Entities;
using AppLogin.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppLogin.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;

        public AccountController(IUserHelper userHelper)
        {
            this._userHelper = userHelper;
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
                
            }

            return View(model);
        }


        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction(nameof(AccountController.Index), "Home");
        }
    }

}