using ArsamBackend.Controllers;
using ArsamBackend.Models;
using ArsamBackend.ViewModels;
using Eventus.UnitTest.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Eventus.UnitTest
{
    public class LoginTest : TestBase
    {
        [Fact]
        public void Login_Successful_200()
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
            var controller = new AccountController(context, mockUserManager.Object, mockSigninManager.Object, mockLogger.Object, mockDPProvider.Object, mockDPPurposeStrings.Object, mockJWTHandler.Object)
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
                    Email = StringGenerators.CreateRandomString() + "@" + StringGenerators.CreateRandomString() + ".com",
                    Password = Pass,
                    RememberMe = RememberMes[new Random().Next(1)]
                };
                Task<IActionResult> result = controller.Login(model);
                Tuple<dynamic, int?> responseBodyStatusCode = getResponse_200(result);
                var Token = responseBodyStatusCode.Item1.token.Value;
                var code = responseBodyStatusCode.Item2.Value;

                #region Assertions
                Assert.Equal("JWT", Token);
                Assert.Equal(200, code);
                #endregion Assertions
            }
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
            var controller = new AccountController(context, mockUserManager.Object, mockSigninManager.Object, mockLogger.Object, mockDPProvider.Object, mockDPPurposeStrings.Object, mockJWTHandler.Object)
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
            var controller = new AccountController(context, mockUserManager.Object, mockSigninManager.Object, mockLogger.Object, mockDPProvider.Object, mockDPPurposeStrings.Object, mockJWTHandler.Object)
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
            var controller = new AccountController(context, mockUserManager.Object, mockSigninManager.Object, mockLogger.Object, mockDPProvider.Object, mockDPPurposeStrings.Object, mockJWTHandler.Object)
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
            var controller = new AccountController(context, mockUserManager.Object, mockSigninManager.Object, mockLogger.Object, mockDPProvider.Object, mockDPPurposeStrings.Object, mockJWTHandler.Object)
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

        

        public Tuple<dynamic, int?> getResponse_200(Task<IActionResult> result)
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
