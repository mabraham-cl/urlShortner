using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using urlShortner.Server;
using urlShortner.Server.Controllers;
using urlShortner.Server.Services;
using urlShortner.Shared;
using Xunit;

namespace urlShortner.UnitTest
{
    public class UrlShortnerController_UnitTest
    {
        [Fact]
        public async Task UrlShortnerController_UnitTest_Get()
        {
            // Arrange
            var mockService = new Mock<IUrlService>();
            mockService.Setup(x => x.GetAsync()).ReturnsAsync(GetUrls());
            var mockIlogger = new Mock<ILogger<UrlShortnerController>>();
            var controller = new UrlShortnerController(mockIlogger.Object, mockService.Object);

            // Act
            var result = await controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<List<LongUrlShortUrl>>(
                okResult.Value);
            Assert.Equal(2, model.Count);
            Assert.Equal("https://www.bbc.co.uk/news", model[0].LongUrl);
            Assert.Equal("aZ1hjdg", model[0].ShortUrl);
            Assert.Equal("gJ1hjdg", model[1].ShortUrl);
        }

        [Fact]
        public async Task UrlShortnerController_UnitTest_Create()
        {
            // Arrange
            var mockService = new Mock<IUrlService>();
            mockService.Setup(x => x.CreateAsync("https://www.bbc.co.uk/news")).ReturnsAsync(new LongUrlShortUrl() { LongUrl = "https://www.bbc.co.uk/news", ShortUrl = "aZ1hjdg" });
            var mockIlogger = new Mock<ILogger<UrlShortnerController>>();
            var controller = new UrlShortnerController(mockIlogger.Object, mockService.Object);

            // Act
            var result = await controller.Create("https://www.bbc.co.uk/news");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<LongUrlShortUrl>(
                okResult.Value);
            Assert.Equal("https://www.bbc.co.uk/news", model.LongUrl);
            Assert.Equal("aZ1hjdg", model.ShortUrl);
        }

        [Fact]
        public async Task UrlShortnerController_UnitTest_Create_InvalidUrl()
        {
            // Arrange
            var mockService = new Mock<IUrlService>();
            mockService.Setup(x => x.CreateAsync("hdshjdshjdfhsfg")).Throws(new UrlShortnerException("Invalid url entered.", 400));
            var mockIlogger = new Mock<ILogger<UrlShortnerController>>();
            var controller = new UrlShortnerController(mockIlogger.Object, mockService.Object);

            // Act
            var result = await controller.Create("hdshjdshjdfhsfg");

            // Assert
            var okResult = Assert.IsType<BadRequestObjectResult>(result);
            var model = Assert.IsAssignableFrom<UrlShortnerErrorResponse>(
                okResult.Value);
            Assert.Equal(400, model.HttpStatus);
            Assert.Equal("Invalid url entered.", model.Message);
        }

        private List<LongUrlShortUrl> GetUrls()
        {
            return new List<LongUrlShortUrl>() { new LongUrlShortUrl() {LongUrl = "https://www.bbc.co.uk/news" , ShortUrl="aZ1hjdg"},
            new LongUrlShortUrl() {LongUrl = "https://www.bbc.co.uk/news1" , ShortUrl="gJ1hjdg"} };
        }
    }
}
