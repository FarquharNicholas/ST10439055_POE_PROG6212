using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;
using ST10439055_POE_PROG6212.Data;
using ST10439055_POE_PROG6212.Services;
using ST10439055_POE_PROG6212.Hubs;

namespace ST10439055_POE_PROG6212
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
           
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();
            
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure file upload settings
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10485760; // 10MB limit
            });

            // Register services
            builder.Services.AddScoped<IFileUploadService, FileUploadService>();

            var app = builder.Build();
            
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapHub<ClaimStatusHub>("/hubs/claimStatus");

            app.Run();
        }
    }
}
