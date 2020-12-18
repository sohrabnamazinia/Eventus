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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Eventus.EventTest
{
    public class EventTest : TestBase
    {
        [Fact]
        public void scenarioOne()
        {
            #region Setup
            var testAppUser = new AppUser() { Email = "test@gmail.com" };
            mockJWTHandler.Setup(x => x.FindUserByTokenAsync(It.IsAny<string>(), eventsContext)).ReturnsAsync(testAppUser);
            mockContext.Setup(x => x.User).Returns(new ClaimsPrincipal());
            mockContext.Setup(x => x.Request.Headers[HeaderNames.Authorization]).Returns("JWT");
            eventService = new EventService(eventsContext, mockEventServiceLogger.Object);
            #endregion Setup

            var controllerContext = new ControllerContext() { HttpContext = mockContext.Object };

            var controller = new EventController(mockJWTHandler.Object, eventsContext, mockEventLogger.Object, eventService) { ControllerContext = controllerContext, Url = mockUrl.Object };

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

            Task<ActionResult> createResult = controller.Create(model);
            Tuple<dynamic, int?> responseBodyStatusCodeCreate = getResponse_200(createResult);
            var eventId = responseBodyStatusCodeCreate.Item1.id.ToString();
            var createCode = responseBodyStatusCodeCreate.Item2.Value;
            Event createdEvent = eventsContext.Events.Find(int.Parse(eventId));

            #region Assertions
            Assert.Equal(200, createCode);
            #endregion Assertions

            //update event scenario

            #region Setup
            mockJWTHandler.Setup(x => x.FindRoleByTokenAsync(It.IsAny<string>(),It.IsAny<int>(), eventsContext)).ReturnsAsync(Role.Admin);
            #endregion Setup
            var updateModel = new InputEventViewModel()
            {
                Name = "updated name unit test",
                IsProject = true,
                Description = "updated description unit test",
                IsPrivate = false,
                StartDate = DateTime.Now.AddHours(1),
                EndDate = DateTime.Now.AddDays(2),
                IsLimitedMember = true,
                MaximumNumberOfMembers = 10,
                Categories = new List<int> { 2 }
            };
            Task<ActionResult> updateResult = controller.Update(createdEvent.Id, updateModel);
            Tuple<dynamic, int?> responseBodyStatusCodeUpdate = getResponse_200(createResult);
            var updatedEventId = responseBodyStatusCodeUpdate.Item1.id.ToString();
            var updateCode = responseBodyStatusCodeUpdate.Item2.Value;
            Event UpdatedEvent = eventsContext.Events.Find(int.Parse(updatedEventId));


            #region Assertions
            Assert.Equal(200, updateCode);
            Assert.Equal("updated name unit test", UpdatedEvent.Name);
            #endregion Assertions

            //get event scenario

            Task<ActionResult> GetResult = controller.Get(int.Parse(updatedEventId));
            Tuple<dynamic, int?> responseBodyStatusCodeGet = getResponse_200(GetResult);
            var responseBody = responseBodyStatusCodeGet.Item1;
            var GetCode = responseBodyStatusCodeGet.Item2.Value;

            #region Assertions
            Assert.Equal(200, GetCode);
            Assert.Equal(UpdatedEvent.Name, responseBody.name.ToString());
            Assert.Equal(updatedEventId, responseBody.id.ToString());
            Assert.Equal(UpdatedEvent.Description, responseBody.description.ToString());
            Assert.Equal(UpdatedEvent.IsPrivate.ToString(), responseBody.isPrivate.ToString());
            Assert.Equal(UpdatedEvent.StartDate.ToString(), responseBody.startDate.ToString());
            Assert.Equal(UpdatedEvent.EndDate.ToString(), responseBody.endDate.ToString());
            Assert.Equal(UpdatedEvent.IsLimitedMember.ToString(), responseBody.isLimitedMember.ToString());
            Assert.Equal(UpdatedEvent.MaximumNumberOfMembers.ToString(), responseBody.maximumNumberOfMembers.ToString());
            Assert.Equal(UpdatedEvent.EndDate.ToString(), responseBody.endDate.ToString());
            #endregion Assertions


            //delete event scenario
            Task<ActionResult> DeleteResult = controller.Delete(int.Parse(updatedEventId));
            int? DeleteCode = GetDeleteResponse(DeleteResult);
            Event DeletedEvent = eventsContext.Events.Find(int.Parse(updatedEventId));
            

            #region Assertions
            Assert.Equal(200, DeleteCode);
            Assert.True(DeletedEvent.IsDeleted);
            #endregion Assertions
        }


        public Tuple<dynamic, int?> getResponse_200(Task<ActionResult> result)
        {
            ObjectResult actionResult = (ObjectResult)result.Result;
            var code = actionResult.StatusCode;
            var serializedBody = serializer.Serialize(actionResult.Value);
            dynamic responseBody = JObject.Parse(serializedBody);
            return new Tuple<dynamic, int?>(responseBody, code);
        }
        private int? GetDeleteResponse(Task<ActionResult> result)
        {
            ObjectResult actionResult = (ObjectResult)result.Result;
            var code = actionResult.StatusCode;
            return code;
        }
    }
}
