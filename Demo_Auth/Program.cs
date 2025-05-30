using Demo_Auth.Data;
using Demo_Auth.Data.Models;
using Demo_Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Demo_Auth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddTransient<TaskService>();

            builder.Services.AddDbContext<TaskManagerDbContext>(option =>
                option.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddDefaultIdentity<AppUser>(option =>
                option.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<TaskManagerDbContext>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
