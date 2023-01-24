using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAuctionApp.Controllers
{
    public class SellerController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("MyAuctions", "Auctions");
        }
    }
}
