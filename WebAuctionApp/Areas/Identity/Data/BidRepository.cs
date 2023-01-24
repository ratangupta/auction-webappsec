using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAuctionApp.Data;
using WebAuctionApp.Models;

namespace WebAuctionApp.Areas.Identity.Data
{
    public class BidRepository
    {
        private readonly WebAuctionAppContext _context;

        public BidRepository(WebAuctionAppContext context)
        {
            _context = context;
        }

        public Bid GetBidById(string id)
        {
            return _context.Bids.Single(a => a.bidID == id);
        }

        public List<Bid> GetAllbids()
        {
            return _context.Bids.OrderBy(a => a.bidID).ToList();
        }

        public List<Bid> GetBidByName(string username)
        {
            return _context.Bids.Where(m => m.userName == username).ToList();
        }

        public Bid Update(Bid bid)
        {
            bid = _context.Bids.Update(bid).Entity;
            _context.SaveChanges();
            return bid;
        }

        public void Add(Bid bid)
        {
            _context.Bids.Add(bid);
            _context.SaveChanges();
        }
    }
}
