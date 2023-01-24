using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WebAuctionApp.Areas.Identity.Data;
using WebAuctionApp.Models;

namespace WebAuctionApp.Data
{
    public class WebAuctionAppContext : IdentityDbContext<AppUser>
    {
        public WebAuctionAppContext(DbContextOptions<WebAuctionAppContext> options)
            : base(options)
        {
        }

        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public override DbSet<AppUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
