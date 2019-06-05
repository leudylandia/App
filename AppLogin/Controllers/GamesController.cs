using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppLogin.Models;
using AppLogin.Models.Entities;
using AppLogin.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace AppLogin.Controllers
{
    [Authorize]
    public class GamesController : Controller
    {
        private readonly LoginDbContext _context;
        private readonly IUserHelper _userHelper;

        public GamesController(LoginDbContext context, IUserHelper userHelper)
        {
            _context = context;
            this._userHelper = userHelper;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Games.ToListAsync());
        }

        public IActionResult Index2()
        {
            return View();
        }

        public async Task<IActionResult> DatosIndex2()
        {
            var resultado = await _context.Games.ToListAsync();
            return Json(resultado);
        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("GamesNotFound");
            }

            var game = await _context.Games
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return new NotFoundViewResult("GamesNotFound");
            }

            return View(game);
        }

        // GET: Games/Create
        [Authorize(Roles ="Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Games/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Console")] Game game)
        {
            if (ModelState.IsValid)
            {
                var correo = User.Identity.Name;
                game.User = await _userHelper.FindUserByEmailAsync(correo);
                //_context.Add(game);
                //await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: Games/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }

        // POST: Games/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Console")] Game game)
        {
            if (id != game.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(game);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: Games/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // POST: Games/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games.FindAsync(id);
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameExists(int id)
        {
            return _context.Games.Any(e => e.Id == id);
        }

        public IActionResult GamesNotFound()
        {
            return View();
        }
    }
}
