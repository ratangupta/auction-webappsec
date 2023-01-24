using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAuctionApp.Data;
using WebAuctionApp.Models;

namespace WebAuctionApp.Areas.Identity.Data
{
    public class AuctionRepository
    {
        private readonly WebAuctionAppContext _context;

        public AuctionRepository(WebAuctionAppContext context)
        {
            _context = context;
        }

        public Auction GetAuctionById(string id)
        {
            return _context.Auctions.Single(a => a.auctionID == id);
        }

        public List<Auction> GetAllAuctions()
        {
            return _context.Auctions.OrderBy(a => a.auctionID).ToList();
        }

        public List<Auction> GetAuctionByName(string username)
        {
            return _context.Auctions.Where(m => m.sellerName == username).ToList();
        }

        public Auction Update(Auction auction)
        {
            auction = _context.Auctions.Update(auction).Entity;
            _context.SaveChanges();
            return auction;
        }

        public void Add(Auction auction)
        {
            _context.Auctions.Add(auction);
            _context.SaveChanges();
        }
    }
}
