using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using WebAuctionApp.Areas.Identity.Data;
using WebAuctionApp.Data;

namespace WebAuctionApp.Utils
{
    public class EmailSender
    {
        public bool send(string to, string subject, string messagebody)
        {
            var message = new MailMessage("support@webauctionapp.com", to)
            {
                Subject = subject,
                IsBodyHtml = true,
                Body = messagebody,
            };
            var client = new SmtpClient() { EnableSsl = true };
            client.Credentials = new System.Net.NetworkCredential("rgupt21@umd.edu", "jtLJR8xImUVBF3Yn");
            client.Host = "smtp-relay.sendinblue.com";
            client.Port = 587;
            try
            {
                client.Send(message);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
