using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebAuctionApp.Areas.Identity.Data;
using WebAuctionApp.Data;
using WebAuctionApp.Models;
using WebAuctionApp.ViewModels;

namespace WebAuctionApp.Controllers
{
    public class BidsController : Controller
    {
        private readonly WebAuctionAppContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<BidsController> _logger;

        public BidsController(WebAuctionAppContext context, UserManager<AppUser> userManager, ILogger<BidsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }


        //Binds all input to Bid model
        [BindProperty]
        public BuyerInputModel Input { get; set; }


        [Authorize(Roles = "Buyer")]
        public class BuyerInputModel
        {
            public float bidAmount { get; set; }
        }


        //GETS the specified bid and displays all details.
        [HttpGet]
        [Authorize(Roles = "Buyer, Seller, Admin")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bid = await _context.Bids
                .Include(b => b.Auction)
                .FirstOrDefaultAsync(m => m.bidID == id);
            if (bid == null)
            {
                return NotFound();
            }

            return View(bid);
        }


        //GETS the create bid page
        [Authorize(Roles = "Buyer")]
        [HttpGet]
        public IActionResult Create(string id)
        {
            Auction auction = _context.Auctions.FirstOrDefault(a => a.auctionID == id);
            if (auction.isActive == false)
            {
                ModelState.AddModelError(string.Empty, "Auction closed.");
                return RedirectToAction("Index", "Buyer");
            }
            return View();
        }

        //Function to create a bid.
        [Authorize(Roles = "Buyer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string id, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                Auction auction = _context.Auctions.FirstOrDefault(a => a.auctionID == id);
                //Cannot bid on a closed auction.
                if (auction.isActive == false)
                {
                    ModelState.AddModelError(string.Empty, "This auction has been closed. Bidding on it is not allowed.");
                    return View();
                }
                else
                {
                    AppUser user = await _userManager.GetUserAsync(User);
                    var checkBid = _context.Bids.Where(u => u.userName == user.UserName && u.auctionID == id).FirstOrDefault();
                    //If the current user has already created a bid on the auction, they cannot create another bid. They must update their bid.
                    if (checkBid != null)
                    {
                        ModelState.AddModelError(string.Empty, "You have already created a bid for this auction. Please go to your homepage to update your existing bid.");
                        return View();
                    }
                    else
                    {
                        if (auction != null)
                        {
                            //minimum bid is calculated by summing the current bid (should be the highest bid at the time) and minimum bid increment.
                            var minBid = auction.currBid + auction.bidIncrement;
                            //check if the created bid is greater than minimum bid, only then allow to create bid.
                            if (Input.bidAmount >= minBid && Input.bidAmount <= auction.endBid)
                            {
                                var bid = new Bid
                                {
                                    userID = user.Id,
                                    userName = user.UserName,
                                    auctionID = id,
                                    bidAmount = Input.bidAmount,
                                };
                                _context.Add(bid);
                                await _context.SaveChangesAsync();
                                _logger.LogInformation("Buyer: " + user.Id + " bid on auction: " + auction.auctionID);

                                auction.currBid = bid.bidAmount;
                                auction.currBidder = user.UserName;

                                _context.Auctions.Update(auction);
                                await _context.SaveChangesAsync();
                                _logger.LogInformation("Auction: " + auction.auctionID + " updated with bid: " + bid.bidID);

                                return RedirectToAction("ConfirmBid", "Bids", bid);
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty,
                                    "Please place a bid between $" + minBid + " and $" + auction.endBid);
                                return View();
                            }
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
            return View();
        }


        //GETS the Edit Bid page
        [Authorize(Roles = "Buyer")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bid = _context.Bids.Where(b=> b.auctionID == id).FirstOrDefault();
            var auction = _context.Auctions.Where(a => a.auctionID == id).FirstOrDefault();
            if (bid == null)
            {
                return RedirectToAction("BidNotFound", "Bids", id);
                
            }

            if (auction.isActive == false)
            {
                ModelState.AddModelError(string.Empty, "Auction closed.");
                return RedirectToAction("Index", "Buyer");
            }
            return View(bid);
        }


        //This function updates bids.
        [Authorize(Roles = "Buyer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string returnurl = null)
        {
            var bid = _context.Bids.Where(b => b.auctionID == id).FirstOrDefault();
            if (bid == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    //Cannot bid on a closed auction.
                    Auction auction = _context.Auctions.FirstOrDefault(a => a.auctionID == id);
                    if (auction.isActive == false)
                    {
                        ModelState.AddModelError(string.Empty, "This auction has been closed. Bidding on it is not allowed.");
                        return View();
                    }
                    else
                    {
                        AppUser user = await _userManager.GetUserAsync(User);
                        var userID = _context.Bids.Where(u => u.userName == user.UserName).Select(x => x.userID).FirstOrDefault();
                        if (userID == null)
                        {
                            //before updating bid, check if buyer has created a bid.
                            ModelState.AddModelError(string.Empty, "You have not yet created a bid for this auction. PLease go to your homepage to create a bid.");
                            return View();
                        }
                        else
                        {
                            if (auction != null)
                            {
                                var minBid = auction.currBid + auction.bidIncrement;
                                //check if the updated bid is greater than minimum bid, only then allow to update bid.
                                if (Input.bidAmount >= minBid && Input.bidAmount <= auction.endBid)
                                {
                                    bid.userID = user.Id;
                                    bid.userName = user.UserName;
                                    bid.auctionID = id;
                                    bid.bidAmount = Input.bidAmount;

                                    _context.Update(bid);
                                    await _context.SaveChangesAsync();
                                    _logger.LogInformation("Bid: " + bid.bidID + " updated.");

                                    auction.currBid = bid.bidAmount;
                                    auction.currBidder = user.UserName;

                                    _context.Auctions.Update(auction);
                                    await _context.SaveChangesAsync();
                                    _logger.LogInformation("Auction: " + auction.auctionID + " updated with bid: " + bid.bidID);
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty,
                                        "Please place a bid between $" + minBid + " and $" + auction.endBid);
                                    return View();
                                }
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BidExists(bid.bidID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("ConfirmBid", "Bids", bid);
            }
            return View(bid);
        }


        [Authorize(Roles = "Buyer")]
        public IActionResult ConfirmBid(Bid bid) => View(bid);


        //Displays a buyer their bids
        [Authorize(Roles = "Buyer, Admin")]
        [HttpGet]
        public async Task<IActionResult> MyBids()
        {
            string username = _userManager.GetUserName(User);
            var auctions = await _context.Auctions.ToListAsync();
            List<MyBids.AuctionBids> auctionBids = new List<MyBids.AuctionBids>();
            foreach (var auction in auctions)
            {
                var id = auction.auctionID;
                var bid = _context.Bids.Where(b => b.userName == username && b.auctionID == id).FirstOrDefault();
                if(bid == null)
                {
                    continue; 
                }
                var auctionBid = new MyBids.AuctionBids()
                {
                    auction = auction,
                    bid = bid
                };
                auctionBids.Add(auctionBid);
            }
            return View(new MyBids { 
                auctionBids = auctionBids,
            });
        }


        //GETS bids for deletion
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bid = await _context.Bids
                .FirstOrDefaultAsync(m => m.userID == id);
            if (bid == null)
            {
                return NotFound();
            }

            return View(bid);
        }


        //This function deletes a bid
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var bid = _context.Bids.Where(b => b.userID == id).FirstOrDefault();
            _logger.LogInformation("Bid: " + bid.bidID + " deleted.");
            _context.Bids.Remove(bid);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Admin");
        }


        //check if bid exists
        private bool BidExists(string id)
        {
            return _context.Bids.Any(e => e.auctionID == id);
        }


        [HttpGet]
        public IActionResult BidNotFound(string id) => View();
    }
}
