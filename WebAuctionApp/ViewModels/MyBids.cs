using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAuctionApp.Models;

namespace WebAuctionApp.ViewModels
{
    public class MyBids
    {
        public List<AuctionBids> auctionBids { get; set; }

        public class AuctionBids
        {
            public Auction auction { get; set; }
            public Bid bid { get; set; }
        }
    }
}
