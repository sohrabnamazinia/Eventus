using ArsamBackend.Controllers;
using ArsamBackend.Models;
using ArsamBackend.ViewModels;
using Eventus.UnitTest.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Eventus.UnitTest
{
    public class CreateTest : TestBase
    {
        [Fact]
        public void Create_Successful_200()
        {
            #region Setup
            
            mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new AppUser());
            mockSigninManager.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            mockJWTHandler.Setup(x => x.GenerateToken(It.IsAny<AppUser>())).Returns("JWT");
            mockContext.Setup(x => x.User).Returns(new ClaimsPrincipal());
            //var httpContext = new DefaultHttpContext();
            //httpContext.Request.Headers[HeaderNames.Authorization] =
            //    "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJhMkB5YWhvby5jb20iLCIxMDA1OCI6IjIiLCJuYmYiOjE2MDc5MzQzMjksImV4cCI6MTYwODAyMDcyOSwiaWF0IjoxNjA3OTM0MzI5fQ.AzamDCWZf9KrmCROERvAHI_X8IZFsFYT_ASAE3vG-AVoFDWDMnhMR5ppsPfk18G-t3v2M7pAUHdEAw8jKTm0aA";
            #endregion Setup

            var controllerContext = new ControllerContext()
            {
                HttpContext = mockContext.Object,
            };

            var controller = new EventController(mockJWTHandler.Object, context, mockEventLogger.Object, eventService.Object)
            {
                ControllerContext = controllerContext,
                Url = mockUrl.Object
            };

            var model = new InputEventViewModel()
            {
                Name = StringGenerators.CreateRandomString(),
                IsProject = true,
                Description = StringGenerators.CreateRandomString(),
                IsPrivate = true,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(2),
                IsLimitedMember = true,
                MaximumNumberOfMembers = 10,
                Categories = new List<int> { 1, 4, 8 }
            };
            Task<ActionResult> result = controller.Create(model);
            Tuple<dynamic, int?> responseBodyStatusCode = getResponse_200(result);
            var Token = responseBodyStatusCode.Item1.token.Value;
            var code = responseBodyStatusCode.Item2.Value;

            #region Assertions
            Assert.Equal("JWT", Token);
            Assert.Equal(200, code);
            #endregion Assertions

        }

        [Fact]
        public void Login_InvalidModelState_400()
        {
            #region Setup
            mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new AppUser());
            mockSigninManager.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            mockJWTHandler.Setup(x => x.GenerateToken(It.IsAny<AppUser>())).Returns("JWT");
            #endregion Setup
            var controllerContext = new ControllerContext()
            {
                HttpContext = mockContext.Object
            };
            var controller = new AccountController(context, mockUserManager.Object, mockSigninManager.Object, mockAccountLogger.Object, mockDPProvider.Object, mockDPPurposeStrings.Object, mockJWTHandler.Object)
            {
                ControllerContext = controllerContext,
                Url = mockUrl.Object
            };
            controller.ModelState.AddModelError(string.Empty, "Invalid ModelState");
            for (int i = 0; i < 100; i++)
            {
                var Pass = StringGenerators.CreateRandomString();
                var RememberMes = new bool[] { true, false };
                var model = new LoginViewModel()
                {
                    Email = StringGenerators.CreateRandomString() + StringGenerators.CreateRandomString(),
                    Password = Pass,
                    RememberMe = RememberMes[new Random().Next(1)]
                };
                Task<IActionResult> result = controller.Login(model);
                int? code = getResponse_400(result);

                #region Assertions
                Assert.Equal(400, code);
                #endregion Assertions
            }
        }

        [Fact]
        public void Login_NotConfirmedEmail_401()
        {
            #region Setup
            mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new AppUser());
            mockSigninManager.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.NotAllowed);
            mockJWTHandler.Setup(x => x.GenerateToken(It.IsAny<AppUser>())).Returns("JWT");
            #endregion Setup
            var controllerContext = new ControllerContext()
            {
                HttpContext = mockContext.Object
            };
            var controller = new AccountController(context, mockUserManager.Object, mockSigninManager.Object, mockAccountLogger.Object, mockDPProvider.Object, mockDPPurposeStrings.Object, mockJWTHandler.Object)
            {
                ControllerContext = controllerContext,
                Url = mockUrl.Object
            };
            for (int i = 0; i < 100; i++)
            {
                controller.ModelState.Clear();
                var Pass = StringGenerators.CreateRandomString();
                var RememberMes = new bool[] { true, false };
                var model = new LoginViewModel()
                {
                    Email = StringGenerators.CreateRandomString() + "@" + StringGenerators.CreateRandomString() + ".com",
                    Password = Pass,
                    RememberMe = RememberMes[new Random().Next(1)]
                };
                Task<IActionResult> result = controller.Login(model);
                int? code = getResponse_401(result);

                #region Assertions
                Assert.Equal(401, code);
                #endregion Assertions
            }
        }

        [Fact]
        public void Login_NoUserFound_404()
        {
            #region Setup
            mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(null, TimeSpan.FromMilliseconds(1));
            #endregion Setup
            var controllerContext = new ControllerContext()
            {
                HttpContext = mockContext.Object
            };
            var controller = new AccountController(context, mockUserManager.Object, mockSigninManager.Object, mockAccountLogger.Object, mockDPProvider.Object, mockDPPurposeStrings.Object, mockJWTHandler.Object)
            {
                ControllerContext = controllerContext,
                Url = mockUrl.Object
            };
            for (int i = 0; i < 100; i++)
            {
                var Pass = StringGenerators.CreateRandomString();
                var RememberMes = new bool[] { true, false };
                var model = new LoginViewModel()
                {
                    Email = StringGenerators.CreateRandomString() + StringGenerators.CreateRandomString(),
                    Password = Pass,
                    RememberMe = RememberMes[new Random().Next(1)]
                };
                Task<IActionResult> result = controller.Login(model);
                int? code = getResponse_404(result);

                #region Assertions
                Assert.Equal(404, code);
                #endregion Assertions
            }
        }

        [Fact]
        public void Login_LockedOutAccount_423()
        {
            #region Setup
            mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new AppUser());
            mockSigninManager.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);
            mockJWTHandler.Setup(x => x.GenerateToken(It.IsAny<AppUser>())).Returns("JWT");
            #endregion Setup
            var controllerContext = new ControllerContext()
            {
                HttpContext = mockContext.Object
            };
            var controller = new AccountController(context, mockUserManager.Object, mockSigninManager.Object, mockAccountLogger.Object, mockDPProvider.Object, mockDPPurposeStrings.Object, mockJWTHandler.Object)
            {
                ControllerContext = controllerContext,
                Url = mockUrl.Object
            };
            for (int i = 0; i < 100; i++)
            {
                controller.ModelState.Clear();
                var Pass = StringGenerators.CreateRandomString();
                var RememberMes = new bool[] { true, false };
                var model = new LoginViewModel()
                {
                    Email = StringGenerators.CreateRandomString() + "@" + StringGenerators.CreateRandomString() + ".com",
                    Password = Pass,
                    RememberMe = RememberMes[new Random().Next(1)]
                };
                Task<IActionResult> result = controller.Login(model);
                int? code = getResponse_423(result);

                #region Assertions
                Assert.Equal(423, code);
                #endregion Assertions
            }
        }



        public Tuple<dynamic, int?> getResponse_200(Task<ActionResult> result)
        {
            OkObjectResult actionResult = (OkObjectResult)result.Result;
            var code = actionResult.StatusCode;
            var serializedBody = serializer.Serialize(actionResult.Value);
            dynamic responseBody = JObject.Parse(serializedBody);
            return new Tuple<dynamic, int?>(responseBody, code);
        }
        private int? getResponse_401(Task<IActionResult> result)
        {
            UnauthorizedObjectResult actionResult = (UnauthorizedObjectResult)result.Result;
            var code = actionResult.StatusCode;
            return code;
        }
        private int? getResponse_404(Task<IActionResult> result)
        {
            NotFoundObjectResult actionResult = (NotFoundObjectResult)result.Result;
            var code = actionResult.StatusCode;
            return code;
        }
        private int? getResponse_400(Task<IActionResult> result)
        {
            BadRequestObjectResult actionResult = (BadRequestObjectResult)result.Result;
            var code = actionResult.StatusCode;
            return code;
        }
        private int? getResponse_423(Task<IActionResult> result)
        {
            ObjectResult actionResult = (ObjectResult)result.Result;
            var code = actionResult.StatusCode;
            return code;
        }
    }
}
