using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10439055_POE_PROG6212.Models;
using ST10439055_POE_PROG6212.Data;
using ST10439055_POE_PROG6212.Services;

namespace ST10439055_POE_PROG6212.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IFileUploadService fileUploadService)
        {
            _logger = logger;
            _context = context;
            _fileUploadService = fileUploadService;
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

                    // Handle file upload if provided
                    if (model.SupportingDocument != null && model.SupportingDocument.Length > 0)
                    {
                        var uploadResult = await _fileUploadService.UploadFileAsync(model.SupportingDocument, claim.ClaimId);
                        if (uploadResult.Success)
                        {
                            var supportingDocument = new SupportingDocument
                            {
                                ClaimId = claim.ClaimId,
                                FileName = uploadResult.FileName,
                                FilePath = uploadResult.FilePath,
                                UploadedAt = DateTime.Now
                            };
                            _context.SupportingDocuments.Add(supportingDocument);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            TempData["WarningMessage"] = $"Claim submitted but file upload failed: {uploadResult.ErrorMessage}";
                            return RedirectToAction(nameof(SubmitClaim));
                        }
                    }

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

        public async Task<IActionResult> UploadDocs()
        {
            var documents = await _context.SupportingDocuments
                .Include(sd => sd.Claim)
                .ThenInclude(c => c.Lecturer)
                .OrderByDescending(sd => sd.UploadedAt)
                .ToListAsync();
            return View(documents);
        }
        
        [HttpGet]
        public async Task<IActionResult> AdminReview()
        {
            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .OrderByDescending(c => c.Month)
                .ToListAsync();
            return View(claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int claimId, string remarks = "")
        {
            try
            {
                var claim = await _context.Claims.FindAsync(claimId);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction(nameof(AdminReview));
                }

                claim.Status = "Approved";
                _context.Claims.Update(claim);

                // Add approval record
                var approval = new Approval
                {
                    ClaimId = claimId,
                    ApprovedBy = "Admin", // In a real app, this would be the logged-in user
                    ApprovalDate = DateTime.Now,
                    Remarks = remarks
                };
                _context.Approvals.Add(approval);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Claim #{claimId} has been approved successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving claim {ClaimId}", claimId);
                TempData["ErrorMessage"] = "An error occurred while approving the claim.";
            }

            return RedirectToAction(nameof(AdminReview));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int claimId, string remarks = "")
        {
            try
            {
                var claim = await _context.Claims.FindAsync(claimId);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction(nameof(AdminReview));
                }

                claim.Status = "Rejected";
                _context.Claims.Update(claim);

                // Add approval record for rejection
                var approval = new Approval
                {
                    ClaimId = claimId,
                    ApprovedBy = "Admin", // In a real app, this would be the logged-in user
                    ApprovalDate = DateTime.Now,
                    Remarks = remarks
                };
                _context.Approvals.Add(approval);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Claim #{claimId} has been rejected.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting claim {ClaimId}", claimId);
                TempData["ErrorMessage"] = "An error occurred while rejecting the claim.";
            }

            return RedirectToAction(nameof(AdminReview));
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
