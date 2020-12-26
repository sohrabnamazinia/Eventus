using ArsamBackend.Controllers;
using ArsamBackend.Models;
using ArsamBackend.Services;
using ArsamBackend.Utilities;
using Eventus.UnitTest.MockedServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Nancy.Json;
using System;
using System.Collections.Generic;
using System.Text;
using ArsamBackend.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace Eventus.UnitTest.TestUtilities
{
    public abstract class TestBase
    {
        #region mock
        public Mock<FakeUserManager> mockUserManager = new Mock<FakeUserManager>();
        public Mock<FakeSignInManager> mockSigninManager = new Mock<FakeSignInManager>();
        public Mock<ILogger<AccountController>> mockLogger = new Mock<ILogger<AccountController>>();
        public Mock<ILogger<EventController>> mockEventLogger = new Mock<ILogger<EventController>>();
        public Mock<ILogger<EventService>> mockEventServiceLogger = new Mock<ILogger<EventService>>();
        public Mock<IDataProtector> mockDataProtector = new Mock<IDataProtector>();
        public Mock<IDataProtectionProvider> mockDPProvider = new Mock<IDataProtectionProvider>();
        public Mock<IJWTService> mockJWTHandler = new Mock<IJWTService>();
        public Mock<DataProtectionPurposeStrings> mockDPPurposeStrings = new Mock<DataProtectionPurposeStrings>();
        public Mock<HttpRequest> mockRequest = new Mock<HttpRequest>();
        public Mock<HttpContext> mockContext = new Mock<HttpContext>();
        public Mock<IUrlHelper> mockUrl = new Mock<IUrlHelper>();
        public JavaScriptSerializer serializer = new JavaScriptSerializer();
        public AppDbContext context = new AppDbContext(new DbContextOptions<AppDbContext>());
        public AppDbContext eventsContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "EventusDbTest").Options);
        public IEventService eventService ;
        public Mock<IMinIOService> MockMinio = new Mock<IMinIOService>();
        public Mock<IEmailService> MockEmailService = new Mock<IEmailService>();
        public Mock<IWebHostEnvironment> MockWebHostEnvironment = new Mock<IWebHostEnvironment>();

        #endregion mock
    }
}
