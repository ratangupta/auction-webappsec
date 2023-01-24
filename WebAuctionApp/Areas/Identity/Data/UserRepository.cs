using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAuctionApp.Data;

namespace WebAuctionApp.Areas.Identity.Data
{
    public class UserRepository
    {
        private readonly WebAuctionAppContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IServiceProvider _serviceProvider;

        public UserRepository(WebAuctionAppContext context, UserManager<AppUser> userManager, IServiceProvider serviceProvider)
        {
            _context = context;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        public Task<AppUser> GetUserByUsername(string username)
        {
            return _userManager.FindByNameAsync(username);
        }

        public async Task<bool> CreateRole(AppUser user)
        {
            var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            Task<IdentityResult> roleResult;
            if (user == null) {
                return false;
            } 
            else
            {
                string assignedRole = user.Role;
                if (assignedRole == "Buyer")
                {
                    var hasBuyerRole = await roleManager.RoleExistsAsync("Buyer");
                    if (!hasBuyerRole)
                    {
                        roleResult = roleManager.CreateAsync(new IdentityRole("Buyer"));
                    }
                    Task<IdentityResult> newUserRole = _userManager.AddToRoleAsync(user, "Buyer");
                }
                else if (assignedRole == "Seller")
                {
                    var hasSellerRole = await roleManager.RoleExistsAsync("Seller");
                    if (!hasSellerRole)
                    {
                        roleResult = roleManager.CreateAsync(new IdentityRole("Seller"));
                    }
                    Task<IdentityResult> newUserRole = _userManager.AddToRoleAsync(user, "Seller");
                }
                return true;
            }
        }
    }
}
