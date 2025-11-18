using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using ST10439055_POE_PROG6212.Data;
using ST10439055_POE_PROG6212.Helpers;
using ST10439055_POE_PROG6212.Models;
using ST10439055_POE_PROG6212.Services;
using System.Linq;

namespace ST10439055_POE_PROG6212.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordService _passwordService;

        public AccountController(ApplicationDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32(SessionKeys.UserId) != null)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Lecturers
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Email.ToLower() == model.Email.ToLower() && l.IsActive);

            if (user == null || !_passwordService.VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32(SessionKeys.UserId, user.LecturerId);
            HttpContext.Session.SetString(SessionKeys.UserName, user.FullName);
            HttpContext.Session.SetString(SessionKeys.UserRole, user.Role.ToString());

            TempData["SuccessMessage"] = $"Welcome back, {user.FullName.Split(' ').First()}!";
            return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

