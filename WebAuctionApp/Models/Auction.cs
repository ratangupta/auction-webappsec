using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebAuctionApp.Models
{
    public class Auction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string auctionID { get; set; }

        [Display(Name = "Seller Name")]
        public string sellerName { get; set; }

        [Display(Name = "Product Name")]
        public string productName { get; set; }

        [Display(Name = "Product Description")]
        public string productDescription { get; set; }

        [Display(Name = "Starting Bid")]
        public float startBid { get; set; }

        [Display(Name = "End Bid")]
        public float endBid { get; set; }

        [Display(Name = "Bid Increment")]
        public float bidIncrement { get; set; }

        [Display(Name = "Auction Close Time")]
        public DateTime bidTime { get; set; }

        [Display(Name = "Current Bid")]
        public float currBid { get; set; }

        [Display(Name = "Current Bidder")]
        public string currBidder { get; set; }

        public bool isActive { get; set; }

        public string imagePath { get; set; }
    }
}
