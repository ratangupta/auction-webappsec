using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebAuctionApp.Areas.Identity.Data;
using WebAuctionApp.Models;

namespace WebAuctionApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }


        [Authorize(Roles = "Seller, Buyer, Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            AppUser user = await _userManager.GetUserAsync(User);
            var role = await _userManager.GetRolesAsync(user);
            if (role[0] == "Seller")
            {
                return RedirectToAction("Index", "Seller");
            }
            else if (role[0] == "Buyer")
            {
                return RedirectToAction("Index", "Buyer");
            }
            else if (role[0] == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            return View();
        }


        [HttpGet]
        public IActionResult About()
        {
            return View();
        }


        [HttpGet]
        public IActionResult ConfirmEmail() => View();


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
