using ArsamBackend.Controllers;
using Eventus.UnitTest.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xunit;

namespace Eventus.UnitTest.AccountTests
{
    public class LogoutTest : TestBase
    {
        [Fact]
        public void Logout_Successful_200()
        {
            #region Setup
            #endregion Setup
            var controllerContext = new ControllerContext()
            {
                HttpContext = mockContext.Object
            };
            var controller = new AccountController(mockUserManager.Object, mockSigninManager.Object, mockLogger.Object, mockDPProvider.Object, mockDPPurposeStrings.Object, mockJWTHandler.Object)
            {
                ControllerContext = controllerContext,
                Url = mockUrl.Object
            };
            for (int i = 0; i < 100; i++)
            {
                Task<IActionResult> result = controller.Logout();
                int? code = getResponse_200(result);

                #region Assertions
                Assert.Equal(200, code);
                #endregion Assertions
            }
        }

        private int? getResponse_200(Task<IActionResult> result)
        {
            OkResult actionResult = (OkResult)result.Result;
            var code = actionResult.StatusCode;
            return code;
        }
    }
}
