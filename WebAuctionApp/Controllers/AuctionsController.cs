using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebAuctionApp.Areas.Identity.Data;
using WebAuctionApp.Data;
using WebAuctionApp.Models;

namespace WebAuctionApp.Controllers
{
    public class AuctionsController : Controller
    {
        private readonly WebAuctionAppContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AuctionsController> _logger;
        private string[] permittedExtensions = { ".jpg", ".jpeg", ".png" }; //whtelisting acceptable file extensions

        public AuctionsController(WebAuctionAppContext context, UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment, ILogger<AuctionsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }


        //Binding all inputs to the Auction model.
        [BindProperty]
        public AuctionInputModel Input { get; set; }

        [Authorize(Roles = "Seller")]
        public class AuctionInputModel
        {
            public string productName { get; set; }
            public string productDescription { get; set; }
            public float startBid { get; set; }
            public float endBid { get; set; }
            public float bidIncrement { get; set; }
            public DateTime bidTime { get; set; }
            public IFormFile imagePath { get; set; } 
        }


        //GETS all auctions and displays(first live auctions and then closed auctions) to the buyer. Homepage for the buyer. 
        [Authorize(Roles = "Buyer")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Auctions.ToListAsync());
        }


        //GETS all the auctions created by the seller and displays in order of live auctions and closed auctions. Homepage for seller.
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> MyAuctions()
        {
            string username = _userManager.GetUserName(User);
            return View(await _context.Auctions.Where(m => m.sellerName == username).ToListAsync());
        }

        
        //Displays all the information of the auction specified by a buyer. 
        [Authorize(Roles = "Buyer, Admin")]
        [HttpGet]
        public async Task<IActionResult> ViewAuction(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.auctionID == id);
            if (auction == null)
            {
                return NotFound();
            }

            return View(auction);
        }


        //Displays all the information of the auction specified by a seller.
        [Authorize(Roles = "Seller, Admin")]
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.auctionID == id);
            if (auction == null)
            {
                return NotFound();
            }

            return View(auction);
        }


        //Calls the Create Auction page.
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //Function to upload product images.
        // File size is limited to 5MB. Sets image path. Generates a random GUID for every image and adds to the image name.
        [HttpPost("Create"), RequestSizeLimit(5242880)]
        private string UploadFiles(AuctionInputModel auction)
        {
            string imgPath = null;
            string uniqueFileName = null;
            string uploadsFolder = null;
            string filePath = null;
            if (auction.imagePath != null)
            {
                //whitelisting extensions
                var ext = Path.GetExtension(Path.GetFileName(auction.imagePath.FileName)).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                {
                    ViewBag.Message = "Warning: Invalid file extension. Cannot upload file";
                }
                else
                {
                    uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot/data/AuctionImages");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(auction.imagePath.FileName); //Sanitizing input filename
                    filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        auction.imagePath.CopyTo(fileStream);
                    }
                    imgPath = uniqueFileName;
                }
            }
            return imgPath;
        }

        
        //Function to create an auction.
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.GetUserAsync(User);
                string imgPath = UploadFiles(Input);

                //End bid amount must be greater than the start amount + minimum bid increment, else throw error.
                if (Input.startBid + Input.bidIncrement < Input.endBid)
                {
                    var auction = new Auction
                    {
                        isActive = true,
                        sellerName = user.UserName,
                        productName = Input.productName,
                        productDescription = Input.productDescription,
                        startBid = Input.startBid,
                        endBid = Input.endBid,
                        bidIncrement = Input.bidIncrement,
                        bidTime = Input.bidTime,
                        currBid = Input.startBid,
                        imagePath = imgPath
                    };
                    _context.Add(auction);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Seller: " + user.Id + " created an auction: " + auction.auctionID);

                    return RedirectToAction("MyAuctions", "Auctions");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Starting bid must be less than ending bid. Ending bid must be greater than starting bid + bid increment.");
                    return View();
                }
            }
            return View();
        }

        
        //Calls the Edit Auction page.
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null)
            {
                return NotFound();
            }

            if (auction.isActive == false)
            {
                ModelState.AddModelError(string.Empty, "Auction closed");
                return RedirectToAction("Index", "Seller");
            }
            return View(auction);
        }


        //Function to edit an auction.
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string? id, string returnurl = null)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                //Try to update auction, if an auction does not exist then throw error.
                try
                {
                    //If an auction is closed, it cannot be updated.
                    if (auction.isActive == false)
                    {
                        ModelState.AddModelError(string.Empty, "Closed auctions cannot be editted.");
                        return View();
                    }
                    else
                    {
                        //End bid amount must be greater than the start amount + minimum bid increment, else throw error.
                        if (Input.startBid + Input.bidIncrement < Input.endBid)
                        {
                            AppUser user = await _userManager.GetUserAsync(User);
                            string imgPath = UploadFiles(Input);
                            auction.isActive = true;
                            auction.sellerName = user.UserName;
                            auction.productName = Input.productName;
                            auction.productDescription = Input.productDescription;
                            auction.startBid = Input.startBid;
                            auction.endBid = Input.endBid;
                            auction.bidIncrement = Input.bidIncrement;
                            auction.bidTime = Input.bidTime;
                            auction.imagePath = imgPath;
                            _context.Update(auction);
                            await _context.SaveChangesAsync();

                            _logger.LogInformation("Seller: " + user.Id + " updated auction: " + auction.auctionID);
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Starting bid must be less than ending bid. Ending bid must be greater than starting bid + bid increment.");
                            return View();
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuctionExists(auction.auctionID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("MyAuctions", "Auctions");
            }
            return View(auction);
        }

        //GETS the specified auction to delete it.
        [Authorize(Roles = "Seller, Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auction = await _context.Auctions
                .FirstOrDefaultAsync(m => m.auctionID == id);
            if (auction == null)
            {
                return NotFound();
            }

            return View(auction);
        }

        //Function to delete an auction.
        [Authorize(Roles = "Seller, Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            _logger.LogInformation("Auction: " + auction.auctionID + " deleted.");

            _context.Auctions.Remove(auction);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyAuctions", "Auctions");
        }

        //Check if auction exists.
        private bool AuctionExists(string id)
        {
            return _context.Auctions.Any(e => e.auctionID == id);
        }
    }
}
