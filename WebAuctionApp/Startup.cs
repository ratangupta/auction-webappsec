using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAuctionApp.Areas.Identity.Data;
using WebAuctionApp.Utils;
using Microsoft.EntityFrameworkCore;
using WebAuctionApp.Data;
using Microsoft.AspNetCore.Http.Features;

namespace WebAuctionApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddDbContext<WebAuctionAppContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("WebAuctionAppContextConnection")));

            //services.AddHostedService<AuctionTimer>();

            services.Configure<IdentityOptions>(options =>
            {
                // SignIn settings
                options.SignIn.RequireConfirmedEmail = true;

                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 5;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(24);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // Token settings.
                options.Tokens.PasswordResetTokenProvider = typeof(MyTotpSecurityStampBasedTokenProvider<AppUser>).Name.Split("`")[0];
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
            });

            services.AddScoped<UserRepository>();
            services.AddScoped<AuctionRepository>();
            services.AddScoped<IHostedService, AuctionTimer>();

            //Default password hasher
            services.AddScoped<IPasswordHasher<IdentityUser>, Argon2Hasher<IdentityUser>>();

            services.Configure<PasswordHasherOptions>(option =>
            {
                option.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
                option.IterationCount = 100000;
            });

            services.Configure<FormOptions>(options =>
            {
                //Limit set to 5MB
                options.MultipartBodyLengthLimit = 5242880;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = false
            });


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Main}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            //Uncomment the following line to create an Admin
            CreateAdmin(serviceProvider);
        }

        private void CreateAdmin(IServiceProvider serviceProvider)
        {

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            Task<IdentityResult> roleResult;
            string email = "admin@webauctionapp.com";

            //Check that there is an Admin role and create if not
            Task<bool> hasAdminRole = roleManager.RoleExistsAsync("Admin");
            hasAdminRole.Wait();

            if (!hasAdminRole.Result)
            {
                roleResult = roleManager.CreateAsync(new IdentityRole("Admin"));
                roleResult.Wait();
            }

            //Check if the admin user exists and create it if not
            //Add to the Admin role

            Task<AppUser> adminUser = userManager.FindByEmailAsync(email);
            adminUser.Wait();
            if (adminUser.Result == null)
            {
                AppUser admin = new AppUser();
                admin.Email = Configuration["Admin:Email"];
                admin.UserName = Configuration["Admin:Username"];
                admin.EmailConfirmed = true;
                string password = Configuration["Admin:Password"];
                admin.Role = "Admin";

                Task<IdentityResult> newUser = userManager.CreateAsync(admin, password);
                newUser.Wait();

                if (newUser.Result.Succeeded)
                {
                    Task<IdentityResult> newUserRole = userManager.AddToRoleAsync(admin, "Admin");
                    newUserRole.Wait();
                }
            }
        }
    }
}
