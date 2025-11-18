using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;
using ST10439055_POE_PROG6212.Data;
using ST10439055_POE_PROG6212.Services;
using ST10439055_POE_PROG6212.Hubs;
using ST10439055_POE_PROG6212.Models;
using ST10439055_POE_PROG6212.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace ST10439055_POE_PROG6212
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
           
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10485760; 
            });

            builder.Services.AddScoped<IFileUploadService, FileUploadService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();

            QuestPDF.Settings.License = LicenseType.Community;

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();

                var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
                
                // Ensure HR super user exists AND has the correct HR role & password
                var hrUser = context.Lecturers.FirstOrDefault(l => l.Email == "hr@cmcs.local");
                passwordService.CreatePasswordHash("Hr@12345", out var hrHash, out var hrSalt);

                if (hrUser == null)
                {
                    hrUser = new Lecturer
                    {
                        FullName = "HR Super User",
                        Email = "hr@cmcs.local",
                        Department = "Human Resources",
                        Role = UserRole.HR,
                        HourlyRate = 0,
                        PasswordHash = hrHash,
                        PasswordSalt = hrSalt,
                        IsActive = true
                    };
                    context.Lecturers.Add(hrUser);
                }
                else
                {
                    hrUser.Role = UserRole.HR;
                    hrUser.IsActive = true;
                    hrUser.HourlyRate = 0;
                    hrUser.PasswordHash = hrHash;
                    hrUser.PasswordSalt = hrSalt;
                }

                SeedDemoAccounts(context, passwordService);
                context.SaveChanges();
            }
            
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapHub<ClaimStatusHub>("/hubs/claimStatus");

            app.Run();
        }

        private static void SeedDemoAccounts(ApplicationDbContext context, IPasswordService passwordService)
        {
            var demoAccounts = new[]
            {
                new { Email = "coordinator@cmcs.local", Name = "Programme Coordinator", Department = "Academic Affairs", Role = UserRole.ProgrammeCoordinator, HourlyRate = 0m, Password = "Coord@123" },
                new { Email = "manager@cmcs.local", Name = "Academic Manager", Department = "Academic Management", Role = UserRole.AcademicManager, HourlyRate = 0m, Password = "Manager@123" },
                new { Email = "lecturer.demo@cmcs.local", Name = "Demo Lecturer", Department = "Computer Science", Role = UserRole.Lecturer, HourlyRate = 450m, Password = "Lecturer@123" }
            };

            foreach (var account in demoAccounts)
            {
                if (context.Lecturers.Any(l => l.Email == account.Email))
                {
                    continue;
                }

                passwordService.CreatePasswordHash(account.Password, out var hash, out var salt);
                context.Lecturers.Add(new Lecturer
                {
                    FullName = account.Name,
                    Email = account.Email,
                    Department = account.Department,
                    Role = account.Role,
                    HourlyRate = account.Role == UserRole.Lecturer ? account.HourlyRate : 0,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    IsActive = true
                });
            }

            context.SaveChanges();
        }
    }
}
