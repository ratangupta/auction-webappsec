using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAuctionApp.Data
{
    public class WebAuctionAppContextFactory : IDesignTimeDbContextFactory<WebAuctionAppContext>
    {
        public WebAuctionAppContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WebAuctionAppContext>();
            optionsBuilder.UseSqlite("Data Source=WebAuctionApp.db");

            return new WebAuctionAppContext(optionsBuilder.Options);
        }
    }
}
