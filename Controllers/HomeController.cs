using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10439055_POE_PROG6212.Models;
using ST10439055_POE_PROG6212.Data;

namespace ST10439055_POE_PROG6212.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index() => View();   // Home Page
        public IActionResult Dashboard() => View();
        
        [HttpGet]
        public IActionResult SubmitClaim()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(ClaimSubmissionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Find or create lecturer
                    var lecturer = await _context.Lecturers
                        .FirstOrDefaultAsync(l => l.Email == model.Email);

                    if (lecturer == null)
                    {
                        lecturer = new Lecturer
                        {
                            FullName = model.LecturerName,
                            Email = model.Email,
                            Department = model.Department,
                            HourlyRate = model.HourlyRate
                        };
                        _context.Lecturers.Add(lecturer);
                        await _context.SaveChangesAsync();
                    }

                    // Create claim
                    var claim = new Claim
                    {
                        LecturerId = lecturer.LecturerId,
                        Month = model.Month,
                        HoursWorked = model.HoursWorked,
                        TotalAmount = model.HoursWorked * model.HourlyRate,
                        Status = "Pending"
                    };

                    _context.Claims.Add(claim);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Claim submitted successfully!";
                    return RedirectToAction(nameof(SubmitClaim));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error submitting claim");
                    ModelState.AddModelError("", "An error occurred while submitting the claim. Please try again.");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> ViewClaims()
        {
            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .OrderByDescending(c => c.Month)
                .ToListAsync();
            return View(claims);
        }

        public IActionResult UploadDocs() => View();
        public IActionResult AdminReview() => View();
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
