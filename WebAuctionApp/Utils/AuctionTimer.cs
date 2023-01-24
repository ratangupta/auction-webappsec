using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebAuctionApp.Areas.Identity.Data;
using WebAuctionApp.Data;
using WebAuctionApp.Models;

namespace WebAuctionApp.Utils
{
    public class AuctionTimer : IHostedService, IDisposable
    {
        private readonly WebAuctionAppContext _context;
        private readonly ILogger<AuctionTimer> _logger;
        private readonly UserManager<AppUser> _userManager;
        private Timer _timer;
        private int executionCount = 0;

        public AuctionTimer(ILogger<AuctionTimer> logger, IServiceScopeFactory factory)
        {
            _logger = logger;
            _context = factory.CreateScope().ServiceProvider.GetRequiredService<WebAuctionAppContext>();
            _userManager = factory.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed service running.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(120));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);
            _logger.LogInformation("Timed service is working. Count: {Count}", count);

            //implement logic here
            var auctions = _context.Auctions.Where(a => a.isActive == true).ToList();
            DateTime localTime = DateTime.Now;
            foreach (var auction in auctions)
            {
                if (localTime > auction.bidTime)
                {
                    auction.isActive = false;
                    if (auction.currBidder != null)
                    {
                        var buyerEmail = _context.Users.Where(u => u.UserName == auction.currBidder).Select(e => e.Email).FirstOrDefault();
                        var sellerEmail = _context.Users.Where(u => u.UserName == auction.sellerName).Select(e => e.Email).FirstOrDefault();
                        EmailSender emailSender = new EmailSender();
                        bool emailBuyerResponse = emailSender.send(buyerEmail, "No-Reply: Your bid won the auction for "+auction.productName,
                            $"Congratulations on your new purchase of "+auction.productName+". This product was auctioned by "+auction.sellerName+". Their email ID is "+sellerEmail+". " +
                            "They will contact you to discuss payment and delivery of the product. Our platform does not provide payment and product tracking services due to security reasons.");
                        bool emailSellerResponse = emailSender.send(sellerEmail, "No Reply: Your product "+auction.productName+" was auctioned.", 
                            $"Your auction "+auction.auctionID+" was successfully completed. "+auction.currBidder+" bought your product. Their email is "+buyerEmail+". "+
                            "Please contact them to discuss payment and delivery of product. Our platform does not provide payment and product tracking services due to security reasons.");
                    }
                    _context.Update(auction);
                    _context.SaveChanges();
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
