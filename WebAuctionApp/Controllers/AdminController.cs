using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAuctionApp.Areas.Identity.Data;
using WebAuctionApp.Data;
using WebAuctionApp.Models;

namespace WebAuctionApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly WebAuctionAppContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(WebAuctionAppContext context, UserManager<AppUser> userManager, ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }


        //GETS all buyers and displays in a table.
        [HttpGet]
        public IActionResult ManageBuyers()
        {
            var role = "Buyer";
            var allBuyers = _userManager.Users.Where(u => u.Role == role);
            return View(allBuyers);
        }

        //Function to delete a buyer. Only Admin has access.
        //This function will get the buyer the specified by admin and will first delete all bids related to the buyer and then the buyer. 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageBuyers(string id, string returnurl = null)
        {
            var allBids = await _context.Bids.Where(b => b.userID == id).ToListAsync();
            var user = await _userManager.FindByIdAsync(id);
            foreach (Bid bid in allBids)
            {
                _context.Bids.Remove(bid);
                await _context.SaveChangesAsync();
            }
            _context.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Admin");
        }

        
        //GETS all sellers and displays in a table.
        [HttpGet]
        public IActionResult ManageSellers()
        {
            var role = "Seller";
            var allSellers = _userManager.Users.Where(u => u.Role == role);
            return View(allSellers);
        }

        //Function to delete a seller. Only Admin has access.
        //This function will get the seller the specified by admin and will first delete all bids related to all the auctions created by the seller.
        //It will then delete all the auctions created by the seller and then the seller.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageSellers(string id, string returnurl = null)
        {
            var user = await _userManager.FindByIdAsync(id);
            var allAuctions = await _context.Auctions.Where(a => a.sellerName == user.UserName).ToListAsync();
            foreach (Auction auction in allAuctions)
            {
                var allBids = await _context.Bids.Where(b => b.auctionID == auction.auctionID).ToListAsync();
                foreach (Bid bid in allBids)
                {
                    _context.Bids.Remove(bid);
                    await _context.SaveChangesAsync();
                }
                _context.Auctions.Remove(auction);
                await _context.SaveChangesAsync();
            }
            _context.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Admin");
        }
    }
}
