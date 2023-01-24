using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using WebAuctionApp.Areas.Identity.Data;

namespace WebAuctionApp.Models
{
    public class Bid
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string bidID { get; set; }

        [ForeignKey("userID")]
        public string userID { get; set; }
        public string userName { get; set; }
        public AppUser AppUser { get; set; }

        [ForeignKey("auctionID")]
        public string auctionID { get; set; }
        public Auction Auction { get; set; }

        [Display(Name = "Bid Amount")]
        public float bidAmount { get; set; }
    }
}
