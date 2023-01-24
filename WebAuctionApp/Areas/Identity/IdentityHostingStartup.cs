using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAuctionApp.Areas.Identity.Data;
using WebAuctionApp.Data;
using WebAuctionApp.Utils;

[assembly: HostingStartup(typeof(WebAuctionApp.Areas.Identity.IdentityHostingStartup))]
namespace WebAuctionApp.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {

                services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<WebAuctionAppContext>()
                    .AddDefaultTokenProviders()
                    .AddTokenProvider<MyTotpSecurityStampBasedTokenProvider<AppUser>>("MyTotpSecurityStampBasedTokenProvider")
                    .AddPasswordValidator<CustomPasswordValidator<AppUser>>();
            });
        }
    }
}