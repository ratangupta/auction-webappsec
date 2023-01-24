using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using WebAuctionApp.Areas.Identity.Data;
using WebAuctionApp.Controllers;
using WebAuctionApp.Data;
using WebAuctionApp.Models;
using WebAuctionApp.ViewModels;
using Xunit;

namespace WebAuctionApp.Tests.Controllers
{
    public class BidsControllerTests
    {
        private WebAuctionAppContext _context;
        private UserManager<AppUser> _userManager;
        private ILogger<BidsController> _logger;
        private readonly BidsController _bidsController;
        //private readonly AuctionsController _auctionsController;

        public BidsControllerTests()
        {
            //Dependecy Injections
            _context = GetDbContext();
            _userManager = A.Fake<UserManager<AppUser>>();
            _logger = A.Fake<ILogger<BidsController>>();

            //Create an add auction to database
            Auction auction = new Auction
            {
                auctionID = "1",
                sellerName = "test",
                productName = "test",
                productDescription = "test",
                startBid = 1,
                endBid = 10,
                bidIncrement = 1,
                bidTime = DateTime.Today.AddMinutes(60),
                currBid = 2,
                currBidder = null,
                isActive = true,
                imagePath = "8643264"
            };
            _context.Auctions.Add(auction);

            //Create and add bid to database
            Bid bid = new Bid
            {
                bidID = "1",
                userID = "1",
                userName = "test",
                auctionID = auction.auctionID,
                bidAmount = 5,
            };
            _context.Bids.Add(bid);

            _context.SaveChanges();

            //SUT
            _bidsController = new BidsController(_context, _userManager, _logger);
        }

        private WebAuctionAppContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<WebAuctionAppContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var databaseContext = new WebAuctionAppContext(options);
            databaseContext.Database.EnsureCreated();
            return databaseContext;
        }

        [Theory]
        [InlineData("1")]
        public void BidsController_Details_ReturnsBid(string id)
        {
            //Arrange

            //Act
            ViewResult result = _bidsController.Details(id).Result as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Bid>(result.Model);
        }

        [Fact]
        public void BidsController_Details_ReturnsNotFound()
        {
            //Arrange
            string id = null;

            //Act
            var result = _bidsController.Details(id).Result;

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData("7")]
        public void BidsController_Details_ReturnsBidNull(string id)
        {
            //Arrange

            //Act
            var result = _bidsController.Details(id).Result;

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData("1")]
        public void BidsController_Create_ReturnsSuccess(string id)
        {
            //Arrange

            //Act
            ViewResult result = _bidsController.Create(id) as ViewResult;

            //Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("1")]
        public void BidsController_Edit_ReturnsBid(string id)
        {
            //Arrange

            //Act
            ViewResult result = _bidsController.Edit(id).Result as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Bid>(result.Model);
        }

        [Fact]
        public void BidsController_Edit_ReturnsNotFound()
        {
            //Arrange
            string id = null;

            //Act
            var result = _bidsController.Edit(id).Result;

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData("2")]
        public void BidsController_Edit_ReturnsBidNotFound(string id)
        {
            //Arrange

            //Act
            var result = _bidsController.Edit(id).Result;

            //Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Theory]
        [InlineData("3")]
        public void BidsController_Edit_ReturnsAuctionClosed(string id)
        {
            //Arrange
            Auction auction = new Auction
            {
                auctionID = "3",
                sellerName = "test",
                productName = "test",
                productDescription = "test",
                startBid = 1,
                endBid = 10,
                bidIncrement = 1,
                bidTime = DateTime.Today.AddMinutes(60),
                currBid = 2,
                currBidder = null,
                isActive = false,
                imagePath = "8643264"
            };
            _context.Auctions.Add(auction);
            _context.SaveChanges();

            //Act
            var result = _bidsController.Edit(id).Result;

            //Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public void BidsController_MyBids_ReturnsBidsWithAuctions()
        {
            //Arrange

            //Act
            ViewResult result = _bidsController.MyBids().Result as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<MyBids>(result.Model);
        }

        [Theory]
        [InlineData("1")]
        public void BidsController_Delete_ReturnsSuccess(string id)
        {
            //Arrange

            //Act
            ViewResult result = _bidsController.Delete(id).Result as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Bid>(result.Model);
        }

        [Fact]
        public void BidsController_Delete_ReturnsNotFound()
        {
            //Arrange
            string id = null;

            //Act
            var result = _bidsController.Delete(id).Result;

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData("5")]
        public void BidsController_Delete_ReturnsBidNull(string id)
        {
            //Arrange

            //Act
            var result = _bidsController.Delete(id).Result;

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        //TODO
        //[Theory]
        //[InlineData("1")]
        //public void BidsController_DeleteConfirmed_ReturnsSuccess(string id)
        //{
        //    //Arrange

        //    //Act
        //    ViewResult result = _bidsController.DeleteConfirmed(id).Result as ViewResult;

        //    //Assert
        //    Assert.IsType<RedirectToActionResult>(result);
        //}

        [Theory]
        [InlineData("1")]
        public void BidsController_BidNotFound_ReturnsSuccess(string id)
        {
            //Arrange

            //Act
            ViewResult result = _bidsController.BidNotFound(id) as ViewResult;

            //Assert
            Assert.NotNull(result);
        }
    }
}
