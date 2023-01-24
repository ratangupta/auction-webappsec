using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebAuctionApp.Areas.Identity.Data;
using WebAuctionApp.Controllers;
using WebAuctionApp.Data;
using WebAuctionApp.Models;
using Xunit;

namespace WebAuctionApp.Tests.Controllers
{
    public class AuctionControllerTests
    {
        private ILogger<AuctionsController> _logger;
        private WebAuctionAppContext _context;
        private IWebHostEnvironment _webHostEnvironment;
        private UserManager<AppUser> _userManager;
        private readonly AuctionsController _auctionsController;

        public AuctionControllerTests()
        {
            //Dependencies
            _logger = A.Fake<ILogger<AuctionsController>>();
            _userManager = A.Fake<UserManager<AppUser>>();
            _webHostEnvironment = A.Fake<IWebHostEnvironment>();
            _context = GetDbContext();

            //Create and add an auction to database
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
            _context.SaveChanges();

            //SUT
            _auctionsController = new AuctionsController(_context, _userManager, _webHostEnvironment, _logger);
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

        [Fact]
        public void AuctionsController_Index_ReturnsAllAucitons()
        {
            //Arrange

            //Act
            ViewResult result = _auctionsController.Index().Result as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<List<Auction>>(result.Model);
        }

        [Fact]
        public void AuctionsController_MyAuctions_ReturnsListOfAuctions()
        {
            //Arrange

            //Act
            ViewResult result = _auctionsController.MyAuctions().Result as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<List<Auction>>(result.Model);
        }

        [Theory]
        [InlineData("1")]
        public void AuctionsController_ViewAuction_ReturnAuction(string id)
        {
            //Arrange

            //Act
            ViewResult result = _auctionsController.ViewAuction(id).Result as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Auction>(result.Model);
        }

        [Fact]
        public void AuctionsController_ViewAuction_ReturnsNotFound()
        {
            //Arrange
            string id = null;

            //Act
            var result = _auctionsController.ViewAuction(id).Result;

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData("1")]
        public void AuctionsController_Details_ReturnsAuction(string id)
        {
            //Arrange

            //Act
            ViewResult result = _auctionsController.Details(id).Result as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Auction>(result.Model);
        }

        [Fact]
        public void AuctionsController_Details_ReturnsNotFound()
        {
            //Arrange
            string id = null;

            //Act
            var result = _auctionsController.Details(id).Result;

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData("1")]
        public void AuctionsController_Create_ReturnsSuccess(string id)
        {
            //Arrange

            //Act
            var result = _auctionsController.Create(id);

            //Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("1")]
        public void AuctionsController_Edit_ReturnsSuccess(string id)
        {
            //Arrange

            //Act
            ViewResult result = _auctionsController.Edit(id).Result as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Auction>(result.Model);
        }

        [Fact]
        public void AuctionsController_Edit_ReturnsNotFound()
        {
            //Arrange
            string id = null;

            //Act
            var result = _auctionsController.Edit(id).Result;

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData("1")]
        public void AuctionsController_Delete_ReturnsSuccess(string id)
        {
            //Arrange

            //Act
            ViewResult result = _auctionsController.Delete(id).Result as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Auction>(result.Model);
        }

        [Fact]
        public void AuctionsController_Delete_ReturnsNotFound()
        {
            //Arrange
            string id = null;

            //Act
            var result = _auctionsController.Delete(id).Result;

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
