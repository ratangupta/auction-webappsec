using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OtpNet;

namespace WebAuctionApp.Utils
{
    public class MyTotpSecurityStampBasedTokenProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser>
        where TUser : class
    {
        public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(false);
        }

        public override async Task<string> GetUserModifierAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            var email = await manager.GetEmailAsync(user);
            return "MyTotp" + purpose + ":" + email;
        }

        public override async Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var secretKey = await manager.CreateSecurityTokenAsync(user);
            var totp = new Totp(secretKey, mode: OtpHashMode.Sha512, step: 180, totpSize: 8);
            var totpCode = totp.ComputeTotp(DateTime.UtcNow);
            return totpCode;
        }

        public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            int code;

            if (!int.TryParse(token, out code))
            {
                return false;
            }

            long timeWindowUsed;

            var secretKey = await manager.CreateSecurityTokenAsync(user);
            var totp = new Totp(secretKey, mode: OtpHashMode.Sha512, step: 180, totpSize: 8);
            var result = totp.VerifyTotp(token, out timeWindowUsed);

            if (result == true && timeWindowUsed == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
