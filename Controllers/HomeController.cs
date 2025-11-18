using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10439055_POE_PROG6212.Models;
using ST10439055_POE_PROG6212.Data;
using ST10439055_POE_PROG6212.Services;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using ST10439055_POE_PROG6212.Helpers;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ST10439055_POE_PROG6212.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly IPasswordService _passwordService;
        private Lecturer? _currentUser;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            IFileUploadService fileUploadService,
            IPasswordService passwordService)
        {
            _logger = logger;
            _context = context;
            _fileUploadService = fileUploadService;
            _passwordService = passwordService;
        }

        public IActionResult Index() => View();

        public IActionResult Dashboard()
        {
            var guard = Guard();
            if (guard != null)
            {
                return guard;
            }

            ViewBag.UserName = CurrentUser?.FullName;
            ViewBag.UserRole = CurrentUser?.Role.ToString();
            return View();
        }

        [HttpGet]
        public IActionResult SubmitClaim()
        {
            var guard = Guard(UserRole.Lecturer);
            if (guard != null)
            {
                return guard;
            }

            var lecturer = CurrentUser!;
            var model = new ClaimSubmissionViewModel
            {
                LecturerName = lecturer.FullName,
                Email = lecturer.Email,
                Department = lecturer.Department,
                HourlyRate = lecturer.HourlyRate,
                Month = DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(ClaimSubmissionViewModel model)
        {
            var guard = Guard(UserRole.Lecturer);
            if (guard != null)
            {
                return guard;
            }

            var lecturer = CurrentUser!;
            model.LecturerName = lecturer.FullName;
            model.Email = lecturer.Email;
            model.Department = lecturer.Department;
            model.HourlyRate = lecturer.HourlyRate;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Month = new DateTime(model.Month.Year, model.Month.Month, 1);

            if (model.HoursWorked > 180)
            {
                ModelState.AddModelError(nameof(model.HoursWorked), "Hours worked cannot exceed 180 per month.");
                return View(model);
            }

            try
            {
                var claim = new Claim
                {
                    LecturerId = lecturer.LecturerId,
                    Month = model.Month,
                    HoursWorked = model.HoursWorked,
                    TotalAmount = lecturer.HourlyRate * model.HoursWorked,
                    Status = "Pending"
                };

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

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
                return RedirectToAction(nameof(ViewClaims));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting claim");
                ModelState.AddModelError("", "An error occurred while submitting the claim. Please try again.");
                return View(model);
            }
        }

        public async Task<IActionResult> ViewClaims()
        {
            var guard = Guard(UserRole.Lecturer);
            if (guard != null)
            {
                return guard;
            }

            var lecturer = CurrentUser!;
            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.LecturerId == lecturer.LecturerId)
                .OrderByDescending(c => c.Month)
                .ToListAsync();
            return View(claims);
        }

        public async Task<IActionResult> UploadDocs()
        {
            var guard = Guard(UserRole.ProgrammeCoordinator, UserRole.AcademicManager, UserRole.HR);
            if (guard != null)
            {
                return guard;
            }

            var documents = await _context.SupportingDocuments
                .Include(sd => sd.Claim)
                .ThenInclude(c => c.Lecturer)
                .OrderByDescending(sd => sd.UploadedAt)
                .ToListAsync();
            return View(documents);
        }

        [HttpGet]
        public async Task<IActionResult> HRDashboard(string? filterMonth = null)
        {
            var guard = Guard(UserRole.HR);
            if (guard != null)
            {
                return guard;
            }

            var monthFilter = ParseMonth(filterMonth);

            var lecturers = await _context.Lecturers
                .OrderBy(l => l.FullName)
                .ToListAsync();

            var approvedClaims = await GetApprovedClaimsAsync(monthFilter);

            var viewModel = new HRDashboardViewModel
            {
                Lecturers = lecturers,
                ApprovedClaims = approvedClaims
            };

            ViewBag.SelectedMonth = monthFilter?.ToString("yyyy-MM");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLecturer(LecturerEditViewModel model)
        {
            var guard = Guard(UserRole.HR);
            if (guard != null)
            {
                return guard;
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Unable to update lecturer. Please ensure all fields are valid.";
                return RedirectToAction(nameof(HRDashboard));
            }

            var lecturer = await _context.Lecturers.FindAsync(model.LecturerId);
            if (lecturer == null)
            {
                TempData["ErrorMessage"] = "Lecturer not found.";
                return RedirectToAction(nameof(HRDashboard));
            }

            lecturer.FullName = model.FullName;
            lecturer.Email = model.Email;
            lecturer.Department = model.Department;
            lecturer.Role = model.Role;
            lecturer.IsActive = model.IsActive;
            lecturer.HourlyRate = model.Role == UserRole.Lecturer ? model.HourlyRate : 0;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                _passwordService.CreatePasswordHash(model.NewPassword, out var hash, out var salt);
                lecturer.PasswordHash = hash;
                lecturer.PasswordSalt = salt;
            }

            try
            {
                _context.Lecturers.Update(lecturer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Lecturer {model.FullName} updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update lecturer {LecturerId}", model.LecturerId);
                TempData["ErrorMessage"] = "An unexpected error occurred while saving lecturer details.";
            }

            return RedirectToAction(nameof(HRDashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserAccountCreateViewModel model)
        {
            var guard = Guard(UserRole.HR);
            if (guard != null)
            {
                return guard;
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Unable to create user. Please fix validation errors.";
                return RedirectToAction(nameof(HRDashboard));
            }

            var emailExists = await _context.Lecturers
                .AnyAsync(l => l.Email.ToLower() == model.Email.ToLower());
            if (emailExists)
            {
                TempData["ErrorMessage"] = "An account with this email already exists.";
                return RedirectToAction(nameof(HRDashboard));
            }

            if (model.Role == UserRole.Lecturer && model.HourlyRate <= 0)
            {
                TempData["ErrorMessage"] = "Lecturer accounts require a positive hourly rate.";
                return RedirectToAction(nameof(HRDashboard));
            }

            var lecturer = new Lecturer
            {
                FullName = model.FullName,
                Email = model.Email,
                Department = model.Department,
                Role = model.Role,
                HourlyRate = model.Role == UserRole.Lecturer ? model.HourlyRate : 0,
                IsActive = true
            };

            _passwordService.CreatePasswordHash(model.Password, out var hash, out var salt);
            lecturer.PasswordHash = hash;
            lecturer.PasswordSalt = salt;

            try
            {
                _context.Lecturers.Add(lecturer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{model.Role} profile created for {model.FullName}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create {Role} user", model.Role);
                TempData["ErrorMessage"] = "An unexpected error occurred while creating the user.";
            }

            return RedirectToAction(nameof(HRDashboard));
        }

        [HttpGet]
        public async Task<IActionResult> DownloadApprovedClaimsReport(string? month = null)
        {
            var guard = Guard(UserRole.HR);
            if (guard != null)
            {
                return guard;
            }

            var filterMonth = ParseMonth(month);
            var rows = await GetApprovedClaimsAsync(filterMonth);

            var sb = new StringBuilder();
            sb.AppendLine("ClaimId,Lecturer,Department,Month,Hours,HourlyRate,TotalAmount");

            foreach (var row in rows.OrderBy(r => r.Month).ThenBy(r => r.LecturerName))
            {
                sb.AppendLine(string.Join(',', new[]
                {
                    row.ClaimId.ToString(),
                    Quote(row.LecturerName),
                    Quote(row.Department),
                    row.Month.ToString("yyyy-MM"),
                    row.HoursWorked.ToString(CultureInfo.InvariantCulture),
                    row.HourlyRate.ToString("F2", CultureInfo.InvariantCulture),
                    row.TotalAmount.ToString("F2", CultureInfo.InvariantCulture)
                }));
            }

            var fileName = filterMonth.HasValue
                ? $"ApprovedClaims_{filterMonth:yyyy_MM}.csv"
                : "ApprovedClaims_All.csv";

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadApprovedClaimsPdf(string? month = null)
        {
            var guard = Guard(UserRole.HR);
            if (guard != null)
            {
                return guard;
            }

            var filterMonth = ParseMonth(month);
            var rows = await GetApprovedClaimsAsync(filterMonth);

            if (!rows.Any())
            {
                TempData["WarningMessage"] = "No approved claims available for the selected period.";
                return RedirectToAction(nameof(HRDashboard));
            }

            var heading = filterMonth.HasValue
                ? $"Approved Claims – {filterMonth:MMMM yyyy}"
                : "Approved Claims – All Periods";

            var pdfBytes = BuildApprovedClaimsPdf(heading, rows);

            var fileName = filterMonth.HasValue
                ? $"ApprovedClaims_{filterMonth:yyyy_MM}.pdf"
                : "ApprovedClaims_All.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        
        [HttpGet]
        public async Task<IActionResult> CoordinatorReview()
        {
            var guard = Guard(UserRole.ProgrammeCoordinator);
            if (guard != null)
            {
                return guard;
            }

            ViewBag.ReviewHeading = "Programme Coordinator Review";
            return View("AdminReview", await BuildClaimReviewModelsAsync());
        }

        [HttpGet]
        public async Task<IActionResult> AcademicManagerReview()
        {
            var guard = Guard(UserRole.AcademicManager);
            if (guard != null)
            {
                return guard;
            }

            ViewBag.ReviewHeading = "Academic Manager Review";
            return View("AdminReview", await BuildClaimReviewModelsAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int claimId, string remarks = "")
        {
            var guard = Guard(UserRole.ProgrammeCoordinator, UserRole.AcademicManager, UserRole.HR);
            if (guard != null)
            {
                return guard;
            }

            try
            {
                var claim = await _context.Claims.FindAsync(claimId);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToReviewerHome();
                }

                claim.Status = "Approved";
                _context.Claims.Update(claim);

              
                var approval = new Approval
                {
                    ClaimId = claimId,
                    ApprovedBy = $"{CurrentUser!.FullName} ({CurrentUser.Role})",
                    ApprovalDate = DateTime.Now,
                    Remarks = remarks
                };
                _context.Approvals.Add(approval);

                await _context.SaveChangesAsync();
                
                await HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<ST10439055_POE_PROG6212.Hubs.ClaimStatusHub>>()
                    .Clients.All.SendAsync("ClaimStatusChanged", claim.ClaimId, claim.Status);
                TempData["SuccessMessage"] = $"Claim #{claimId} has been approved successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving claim {ClaimId}", claimId);
                TempData["ErrorMessage"] = "An error occurred while approving the claim.";
            }

            return RedirectToReviewerHome();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int claimId, string remarks = "")
        {
            var guard = Guard(UserRole.ProgrammeCoordinator, UserRole.AcademicManager, UserRole.HR);
            if (guard != null)
            {
                return guard;
            }

            try
            {
                var claim = await _context.Claims.FindAsync(claimId);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToReviewerHome();
                }

                claim.Status = "Rejected";
                _context.Claims.Update(claim);

              
                var approval = new Approval
                {
                    ClaimId = claimId,
                    ApprovedBy = $"{CurrentUser!.FullName} ({CurrentUser.Role})",
                    ApprovalDate = DateTime.Now,
                    Remarks = remarks
                };
                _context.Approvals.Add(approval);

                await _context.SaveChangesAsync();
              
                await HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<ST10439055_POE_PROG6212.Hubs.ClaimStatusHub>>()
                    .Clients.All.SendAsync("ClaimStatusChanged", claim.ClaimId, claim.Status);
                TempData["SuccessMessage"] = $"Claim #{claimId} has been rejected.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting claim {ClaimId}", claimId);
                TempData["ErrorMessage"] = "An error occurred while rejecting the claim.";
            }

            return RedirectToReviewerHome();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static IEnumerable<ClaimVerificationResult> EvaluateClaim(Claim claim, bool hasDuplicateMonthSubmission)
        {
            var results = new List<ClaimVerificationResult>();

            if (claim.HoursWorked > 180)
            {
                results.Add(new ClaimVerificationResult
                {
                    RuleName = "Hours Worked",
                    Status = ClaimVerificationStatus.Fail,
                    Message = "Hours exceed the hard limit of 180 per month."
                });
            }
            else if (claim.HoursWorked > 160)
            {
                results.Add(new ClaimVerificationResult
                {
                    RuleName = "Hours Worked",
                    Status = ClaimVerificationStatus.Warning,
                    Message = "Hours exceed the recommended 160 per month. Please verify workload justification."
                });
            }
            else
            {
                results.Add(new ClaimVerificationResult
                {
                    RuleName = "Hours Worked",
                    Status = ClaimVerificationStatus.Pass,
                    Message = "Hours fall within the recommended range."
                });
            }

            var expectedTotal = claim.Lecturer.HourlyRate * claim.HoursWorked;
            if (Math.Abs(expectedTotal - claim.TotalAmount) > 0.01m)
            {
                results.Add(new ClaimVerificationResult
                {
                    RuleName = "Hourly Rate Alignment",
                    Status = ClaimVerificationStatus.Fail,
                    Message = "Total does not match the lecturer's registered hourly rate."
                });
            }
            else
            {
                results.Add(new ClaimVerificationResult
                {
                    RuleName = "Hourly Rate Alignment",
                    Status = ClaimVerificationStatus.Pass,
                    Message = "Payment aligns with the lecturer's approved hourly rate."
                });
            }

            if (claim.SupportingDocuments.Any())
            {
                results.Add(new ClaimVerificationResult
                {
                    RuleName = "Supporting Documents",
                    Status = ClaimVerificationStatus.Pass,
                    Message = $"{claim.SupportingDocuments.Count} document(s) uploaded."
                });
            }
            else
            {
                results.Add(new ClaimVerificationResult
                {
                    RuleName = "Supporting Documents",
                    Status = ClaimVerificationStatus.Fail,
                    Message = "No supporting documents uploaded."
                });
            }

            if (claim.Month.Date > DateTime.UtcNow.Date)
            {
                results.Add(new ClaimVerificationResult
                {
                    RuleName = "Submission Period",
                    Status = ClaimVerificationStatus.Fail,
                    Message = "Claim submitted for a future period."
                });
            }
            else
            {
                results.Add(new ClaimVerificationResult
                {
                    RuleName = "Submission Period",
                    Status = ClaimVerificationStatus.Pass,
                    Message = "Submission month is valid."
                });
            }

            if (hasDuplicateMonthSubmission)
            {
                results.Add(new ClaimVerificationResult
                {
                    RuleName = "Duplicate Submission",
                    Status = ClaimVerificationStatus.Warning,
                    Message = "Multiple claims detected for this lecturer and month."
                });
            }

            return results;
        }

        private async Task<List<ClaimReviewViewModel>> BuildClaimReviewModelsAsync()
        {
            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.SupportingDocuments)
                .OrderByDescending(c => c.Month)
                .ToListAsync();

            var duplicateLookup = claims
                .GroupBy(c => $"{c.LecturerId}-{c.Month:yyyyMM}")
                .ToDictionary(g => g.Key, g => g.Count() > 1);

            return claims.Select(c =>
            {
                var key = $"{c.LecturerId}-{c.Month:yyyyMM}";
                var hasDuplicate = duplicateLookup.TryGetValue(key, out var dup) && dup;
                return new ClaimReviewViewModel
                {
                    Claim = c,
                    VerificationResults = EvaluateClaim(c, hasDuplicate)
                };
            }).ToList();
        }

        private async Task<List<ApprovedClaimReportRow>> GetApprovedClaimsAsync(DateTime? filterMonth)
        {
            var query = _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == "Approved");

            if (filterMonth.HasValue)
            {
                var monthStart = new DateTime(filterMonth.Value.Year, filterMonth.Value.Month, 1);
                var monthEnd = monthStart.AddMonths(1);
                query = query.Where(c => c.Month >= monthStart && c.Month < monthEnd);
            }

            return await query
                .OrderByDescending(c => c.Month)
                .Select(c => new ApprovedClaimReportRow
                {
                    ClaimId = c.ClaimId,
                    LecturerName = c.Lecturer.FullName,
                    Department = c.Lecturer.Department,
                    Month = c.Month,
                    HoursWorked = c.HoursWorked,
                    HourlyRate = c.Lecturer.HourlyRate,
                    TotalAmount = c.TotalAmount
                })
                .ToListAsync();
        }

        private static byte[] BuildApprovedClaimsPdf(string heading, IEnumerable<ApprovedClaimReportRow> rows)
        {
            var orderedRows = rows.OrderBy(r => r.Month).ThenBy(r => r.LecturerName).ToList();
            var totalAmount = orderedRows.Sum(r => r.TotalAmount);
            var totalHours = orderedRows.Sum(r => r.HoursWorked);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);

                    page.Header()
                        .Text("CMCS Payroll Automation Report")
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.Blue.Medium);

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text(heading).FontSize(14).SemiBold();
                        col.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken2);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(1.4f);
                                columns.RelativeColumn(1f);
                                columns.RelativeColumn(0.7f);
                                columns.RelativeColumn(0.7f);
                                columns.RelativeColumn(0.8f);
                            });

                            static IContainer CellStyle(IContainer container) =>
                                container.PaddingVertical(4).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Claim #").Bold();
                                header.Cell().Element(CellStyle).Text("Lecturer").Bold();
                                header.Cell().Element(CellStyle).Text("Department").Bold();
                                header.Cell().Element(CellStyle).Text("Month").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Hours").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Total (R)").Bold();
                            });

                            foreach (var row in orderedRows)
                            {
                                table.Cell().Element(CellStyle).Text(row.ClaimId.ToString());
                                table.Cell().Element(CellStyle).Text(row.LecturerName);
                                table.Cell().Element(CellStyle).Text(row.Department);
                                table.Cell().Element(CellStyle).Text(row.MonthDisplay);
                                table.Cell().Element(CellStyle).AlignRight().Text(row.HoursWorked.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text(row.TotalAmount.ToString("F2"));
                            }
                        });

                        col.Item().Border(1).BorderColor(Colors.Blue.Lighten2).Padding(10).Column(summary =>
                        {
                            summary.Item().Text($"Total Hours: {totalHours}").SemiBold();
                            summary.Item().Text($"Total Payout: R {totalAmount:F2}").SemiBold();
                        });
                    });
                });
            }).GeneratePdf();
        }

        private static DateTime? ParseMonth(string? monthValue)
        {
            if (string.IsNullOrWhiteSpace(monthValue))
            {
                return null;
            }

            if (DateTime.TryParse($"{monthValue}-01", out var parsed))
            {
                return new DateTime(parsed.Year, parsed.Month, 1);
            }

            return null;
        }

        private Lecturer? CurrentUser => _currentUser ??= LoadCurrentUser();

        private Lecturer? LoadCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            if (!userId.HasValue)
            {
                return null;
            }

            return _context.Lecturers.FirstOrDefault(l => l.LecturerId == userId.Value && l.IsActive);
        }

        private IActionResult? Guard(params UserRole[] allowedRoles)
        {
            var user = CurrentUser;
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (allowedRoles != null && allowedRoles.Length > 0 && user.Role != UserRole.HR && !allowedRoles.Contains(user.Role))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return null;
        }

        private IActionResult RedirectToReviewerHome()
        {
            return CurrentUser?.Role switch
            {
                UserRole.ProgrammeCoordinator => RedirectToAction(nameof(CoordinatorReview)),
                UserRole.AcademicManager => RedirectToAction(nameof(AcademicManagerReview)),
                UserRole.HR => RedirectToAction(nameof(HRDashboard)),
                UserRole.Lecturer => RedirectToAction(nameof(ViewClaims)),
                _ => RedirectToAction(nameof(Dashboard))
            };
        }

        private static string Quote(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            var escaped = value.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }
    }
}
