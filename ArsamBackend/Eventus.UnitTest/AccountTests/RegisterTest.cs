using ArsamBackend.Controllers;
using ArsamBackend.Models;
using ArsamBackend.ViewModels;
using Eventus.UnitTest.TestUtilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Eventus.UnitTest
{
    public class RegisterTest : TestBase
    {
        [Fact]
        public void Register_ValidModelState_201()
        {
            #region Setup
            mockDataProtector.Setup(sut => sut.Protect(It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("Protected string"));
            mockDataProtector.Setup(sut => sut.Unprotect(It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("Original String"));
            mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).Returns(() => null);
            mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>())).Returns(Task.FromResult(IdentityResult.Success));
            mockDPProvider.Setup(s => s.CreateProtector(It.IsAny<string>())).Returns(mockDataProtector.Object);
            mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<AppUser>())).ReturnsAsync("Email Confirmation Token");
            mockContext.SetupGet(x => x.Request).Returns(mockRequest.Object);
            mockContext.SetupGet(x => x.Request.Scheme).Returns("BaseUrl");
            #endregion Setup
            var controllerContext = new ControllerContext()
            {
                HttpContext = mockContext.Object
            };
            var controller = new AccountController(context, mockUserManager.Object, mockSigninManager.Object, mockLogger.Object, mockDPProvider.Object, mockDPPurposeStrings.Object, mockJWTHandler.Object, MockMinio.Object, MockEmailService.Object, MockWebHostEnvironment.Object)
            {
                ControllerContext = controllerContext,
                Url = mockUrl.Object
            };
            for (int i = 0; i < 100; i++)
            {
                var Pass = StringGenerators.CreateRandomString();
                var model = new RegisterViewModel()
                {
                    EmailAddress = StringGenerators.CreateRandomString() + "@" + StringGenerators.CreateRandomString() + ".com",
                    Password = Pass,
                    PasswordConfirmation = Pass
                };
                Task<ActionResult<AppUser>> result = controller.Register(model);
                Tuple<dynamic, int?> responseBodyStatusCode = getResponse_201(result);
                var responseBody = responseBodyStatusCode.Item1;
                var code = responseBodyStatusCode.Item2;
                var email = responseBody.email.Value;
                var Token = responseBody.token.Value;
                #region Assertions
                Assert.Equal(201, code);
                Assert.Equal(model.EmailAddress, email);
                Assert.Equal("Email Confirmation Token", Token);
                #endregion Assertions
            }
        }

        [Fact]
        public void Register_InvalidModelState_400()
        {
            #region Setup
            mockDataProtector.Setup(sut => sut.Protect(It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("Protected string"));
            mockDataProtector.Setup(sut => sut.Unprotect(It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("Original String"));
            mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).Returns(() => null);
            mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>())).Returns(Task.FromResult(IdentityResult.Success));
            mockDPProvider.Setup(s => s.CreateProtector(It.IsAny<string>())).Returns(mockDataProtector.Object);
            mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<AppUser>())).ReturnsAsync("Email Confirmation Token");
            mockContext.SetupGet(x => x.Request).Returns(mockRequest.Object);
            mockContext.SetupGet(x => x.Request.Scheme).Returns("BaseUrl");
            #endregion Setup
            var controllerContext = new ControllerContext()
            {
                HttpContext = mockContext.Object
            };
            var controller = new AccountController(context, mockUserManager.Object, mockSigninManager.Object, mockLogger.Object, mockDPProvider.Object, mockDPPurposeStrings.Object, mockJWTHandler.Object, MockMinio.Object, MockEmailService.Object, MockWebHostEnvironment.Object)
            {
                ControllerContext = controllerContext,
                Url = mockUrl.Object
            };
            controller.ModelState.AddModelError(string.Empty, "Invalid Model State");
            for (int i = 0; i < 100; i++)
            {
                var Pass = StringGenerators.CreateRandomString();
                var model = new RegisterViewModel()
                {
                    EmailAddress = StringGenerators.CreateRandomString() + StringGenerators.CreateRandomString(),
                    Password = Pass,
                    PasswordConfirmation = Pass
                };
                Task<ActionResult<AppUser>> result = controller.Register(model);
                var code = getResponse_400(result);

                #region Assertions
                Assert.Equal(400, code);
                #endregion Assertions
            }
        }

        public Tuple<dynamic, int?> getResponse_201(Task<ActionResult<AppUser>> result)
        {
            CreatedAtActionResult actionResult = (CreatedAtActionResult)result.Result.Result;
            var code = actionResult.StatusCode;
            var serializedBody = serializer.Serialize(actionResult.Value);
            dynamic responseBody = JObject.Parse(serializedBody);
            return new Tuple<dynamic, int?>(responseBody, code);
        }

        public int? getResponse_400(Task<ActionResult<AppUser>> result)
        {
            BadRequestObjectResult actionResult = (BadRequestObjectResult)result.Result.Result;
            var code = actionResult.StatusCode;
            return code;
        }
    }
}
