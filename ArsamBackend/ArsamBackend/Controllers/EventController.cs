using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using ActionFilters.ActionFilters;
using ArsamBackend.Models;
using ArsamBackend.Security;
using ArsamBackend.Services;
using ArsamBackend.Utilities;
using ArsamBackend.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using MediaTypeHeaderValue = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;
using Task = System.Threading.Tasks.Task;

namespace ArsamBackend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventService _eventService;
        private readonly IMinIOService minIOService;
        private readonly IJWTService jwtHandler;
        private readonly AppDbContext _context;

        public EventController(IJWTService jwtHandler, AppDbContext context, ILogger<EventController> logger, IEventService eventService, IMinIOService minIO)
        {
            _logger = logger;
            this._eventService = eventService;
            this.jwtHandler = jwtHandler;
            this._context = context;
            this.minIOService = minIO;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Create(InputEventViewModel incomeEvent)
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            if (requestedUser == null)
                return StatusCode(401, "token is invalid, user not found");

            if (requestedUser.ExpireDateOfPremium == null && requestedUser.CreatedEvents.Count >= 5)
                return StatusCode(402, "upgrade To Premium to create more events");
            
            Event createdEvent = await _eventService.CreateEvent(incomeEvent, requestedUser);

            var result = new OutputEventViewModel(createdEvent);
            return Ok(result);
        }

        [Authorize]
        [ServiceFilter(typeof(NotBlocked))]
        [HttpPost]
        public async Task<ActionResult> AddImage(int eventId)
        {
            Role? userRole = await jwtHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], eventId, _context);

            Event existEvent = await _context.Events.FindAsync(eventId);
            if (existEvent == null || existEvent.IsDeleted)
                return NotFound("no event found by this id: " + eventId);

            if (userRole != Role.Admin)
                return StatusCode(403, "access denied");

            var files = Request.Form.Files;
            if (files.Count + existEvent.Images.Count > 5) return BadRequest("can not add more than 5 image to each event");

            foreach (var file in files)
            {
                if (file != null && file.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        if (!Constants.FileFormatChecker(fileBytes) ||
                            !Constants.CheckFileNameExtension(Path.GetExtension(file.FileName)))
                            return StatusCode(415, "one of file contents is not a valid format!");
                    }

                    if (file.Length > 5 * Math.Pow(10, 6))
                        return BadRequest("image size limit is 5 mg");
                }
                else
                    return BadRequest("image not found");
            }
            foreach (var file in files)
            {
                await minIOService.AddImageToEvent(file, existEvent);
            }

            if (existEvent.Images.Count > 0)
            {
                foreach (var img in existEvent.Images)
                    img.ImageLink = minIOService.GenerateEventsUrl(img.FileName).Result;
                await _context.SaveChangesAsync();
            }

            var result = new OutputEventViewModel(existEvent);
            return Ok(result);

        }

        [Authorize]
        [ServiceFilter(typeof(NotBlocked))]
        [HttpPut]
        public async Task<ActionResult> UpdateImage(int eventId, int imageId)
        {
            Event existEvent = await _context.Events.FindAsync(eventId);
            if (existEvent == null || existEvent.IsDeleted)
                return NotFound("no event found by this id: " + eventId);

            Role? userRole = await jwtHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], eventId, _context);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied");

            var files = Request.Form.Files;
            if (files.Count != 1) return BadRequest(Constants.OneImageRequiredError);

            var newFile = files[0];

            if (newFile != null && newFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    newFile.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    if (!Constants.FileFormatChecker(fileBytes) || !Constants.CheckFileNameExtension(Path.GetExtension(newFile.FileName))) return StatusCode(415, "File content is not a valid format!");
                }

                if (newFile.Length > 5 * Math.Pow(10, 6))
                    return BadRequest("image size limit is 5 mg");

                EventImage oldImage = existEvent.Images.SingleOrDefault(x => x.Id == imageId);
                if (oldImage == null) return NotFound("no image found by this imageId");

                await minIOService.UpdateEventImage(newFile, oldImage);
            }
            else
                return BadRequest("image not found");


            if (existEvent.Images.Count > 0)
            {
                foreach (var img in existEvent.Images)
                    img.ImageLink = minIOService.GenerateEventsUrl(img.FileName).Result;
                await _context.SaveChangesAsync();
            }
            var result = new OutputEventViewModel(existEvent);
            return Ok(result);
        }

        [Authorize]
        [ServiceFilter(typeof(NotBlocked))]
        [HttpDelete]
        public async Task<ActionResult> DeleteImage(int eventId, int imageId)
        {
            Event existEvent = await _context.Events.FindAsync(eventId);
            if (existEvent == null || existEvent.IsDeleted)
                return NotFound("no event found by this id: " + eventId);

            Role? userRole = await jwtHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], eventId, _context);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied");

            EventImage oldImage = existEvent.Images.SingleOrDefault(x => x.Id == imageId);
            if (oldImage == null) return NotFound("no image found by this imageId");

            await minIOService.DeleteEventImage(oldImage);

            if (existEvent.Images.Count > 0)
            {
                foreach (var img in existEvent.Images)
                    img.ImageLink = minIOService.GenerateEventsUrl(img.FileName).Result;
                await _context.SaveChangesAsync();
            }

            var result = new OutputEventViewModel(existEvent);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult> Get(int id)
        {
            Event resultEvent = await _context.Events.FindAsync(id);
            if (resultEvent == null || resultEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            Role? userRole = await jwtHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], id, _context);

            if (resultEvent.IsPrivate && userRole == null)
                return StatusCode(403, "access denied, this event is private");

            if (resultEvent.Images.Count > 0)
            {
                foreach (var img in resultEvent.Images)
                    img.ImageLink = minIOService.GenerateEventsUrl(img.FileName).Result;
                await _context.SaveChangesAsync();
            }

            if (userRole == Role.Admin)
            {
                List<AppUser> admins = _context.EventUserRole.Where(x => x.EventId == id && x.Role == Role.Admin && !x.IsDeleted)
                    .Select(x => x.AppUser).ToList();
                AdminOutputEventViewModel adminResult = new AdminOutputEventViewModel(resultEvent, admins, userRole);
                return Ok(adminResult);
            }

            OutputEventViewModel result = new OutputEventViewModel(resultEvent, userRole);
            return Ok(result);
        }

        [Authorize]
        [ServiceFilter(typeof(NotBlocked))]
        [HttpPut]
        public async Task<ActionResult> Update(int id, InputEventViewModel incomeEvent)
        {
            Event existEvent = await _context.Events.FindAsync(id);
            if (existEvent == null || existEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            Role? userRole = await jwtHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], id, _context);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            existEvent.Name = incomeEvent.Name;
            existEvent.Description = incomeEvent.Description;
            existEvent.IsPrivate = incomeEvent.IsPrivate;
            existEvent.StartDate = incomeEvent.StartDate;
            existEvent.EndDate = incomeEvent.EndDate;
            existEvent.IsLimitedMember = incomeEvent.IsLimitedMember;
            existEvent.MaximumNumberOfMembers = incomeEvent.MaximumNumberOfMembers;
            existEvent.Categories = CategoryService.BitWiseOr(incomeEvent.Categories);
            existEvent.BuyingTicketEnabled = incomeEvent.BuyingTicketEnabled;

            if (existEvent.Images.Count > 0)
            {
                foreach (var img in existEvent.Images)
                    img.ImageLink = minIOService.GenerateEventsUrl(img.FileName).Result;
            }
            await _context.SaveChangesAsync();

            var result = new OutputEventViewModel(existEvent);
            return Ok(result);
        }

        [Authorize]
        [ServiceFilter(typeof(NotBlocked))]
        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);

            Event existEvent = await _context.Events.SingleOrDefaultAsync(x => x.Id == id);
            if (existEvent == null || existEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            Role? userRole = await jwtHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], id, _context);
            
            if (userRole != Role.Admin && requestedUser != existEvent.Creator)
                return StatusCode(403, "access denied, you are not an admin");

            existEvent.IsDeleted = true;
            

            await _context.SaveChangesAsync();
            return Ok("event deleted");
        }

        [Authorize]
        [ServiceFilter(typeof(NotBlocked))]
        [HttpPost]
        public async Task<ActionResult> JoinRequest(int eventId)
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);

            Event existEvent = await _context.Events.FindAsync(eventId);
            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "Event Not Found");

            Role? userRole = await jwtHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], eventId, _context);
            if (userRole != null)
                return BadRequest("You are in this event already");

            
            var userRoleInDb = await _context.EventUserRole.IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.AppUserId == requestedUser.Id && x.EventId == existEvent.Id);

            if (userRoleInDb == null)
            {
                var memberRoleRequest = new EventUserRole() { AppUser = requestedUser, AppUserId = requestedUser.Id, Event = existEvent, EventId = existEvent.Id, Role = Role.Member, Status = UserRoleStatus.Pending };
                await _context.EventUserRole.AddAsync(memberRoleRequest);
                await _context.SaveChangesAsync();
                return Ok("Your request has been sent");
            }
            else if (userRoleInDb.IsDeleted)
            {
                _context.EventUserRole.Remove(userRoleInDb);//remove last information 
                var memberRoleRequest = new EventUserRole() { AppUser = requestedUser, AppUserId = requestedUser.Id, Event = existEvent, EventId = existEvent.Id, Role = Role.Member, Status = UserRoleStatus.Pending };
                await _context.EventUserRole.AddAsync(memberRoleRequest);
                await _context.SaveChangesAsync();
                return Ok("Your request has been sent");
            }

            if (userRoleInDb.Status == UserRoleStatus.Rejected)
            {
                userRoleInDb.Status = UserRoleStatus.Pending;
                userRoleInDb.DateOfRequest = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok("Your request has been sent again");
            }
            else if (userRoleInDb.Status == UserRoleStatus.Pending)
                return BadRequest("Your join request has been sent before it's pending for accept");
            
            return BadRequest("wtf!");
        }

        [Authorize]
        [ServiceFilter(typeof(NotBlocked))]
        [HttpGet]
        public async Task<ActionResult> GetJoinRequests(int eventId)
        {
            Event existEvent = await _context.Events.FindAsync(eventId);
            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            Role? userRole = await jwtHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], eventId, _context);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            var joinRequests = _context.EventUserRole.Where(x =>
                (x.Event.Id == eventId) && (x.Status == UserRoleStatus.Pending) && (x.Role == Role.Member) && (!x.IsDeleted)).ToList();

            var results = joinRequests.Select(x => new OutputJoinRequestViewModel(x)).ToList();
            return Ok(results);
        }


        [Authorize]
        [ServiceFilter(typeof(NotBlocked))]
        [HttpPatch]
        public async Task<ActionResult> AcceptOrRejectJoinRequest(int eventId, string memberEmail, bool accept)
        {
            Event existEvent = await _context.Events.FindAsync(eventId);
            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            Role? userRole = await jwtHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], eventId, _context);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            AppUser member = await _context.Users.SingleOrDefaultAsync(c => c.Email == memberEmail);
            if (member == null)
                return StatusCode(404, "user with this email not found");

            var userRoleInDb = await _context.EventUserRole.FindAsync(member.Id, eventId);
            if (userRoleInDb == null || userRoleInDb.IsDeleted)
                return BadRequest("this user has no join request");

            if (userRoleInDb.Status == UserRoleStatus.Accepted)
                return BadRequest("this user is already accepted");
            else if (userRoleInDb.Status == UserRoleStatus.Rejected)
                return BadRequest("this user is already rejected");

            if (!accept)
            {
                userRoleInDb.Status = UserRoleStatus.Rejected;
                await _context.SaveChangesAsync();
                return Ok("user joinRequest rejected successfully");
            }

            else
            {
                userRoleInDb.Status = UserRoleStatus.Accepted;
                var membersList = existEvent.EventMembers.ToList();
                membersList.Add(member);
                existEvent.EventMembers = membersList;
                await _context.SaveChangesAsync();
                return Ok("accepted, this user is a member now");
            }
        }

        [Authorize]
        [ServiceFilter(typeof(NotBlocked))]
        [HttpPatch]
        public async Task<ActionResult> PromoteMember(int id, string memberEmail)
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            Event existEvent = await _context.Events.FindAsync(id);

            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            Role? userRole = await jwtHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], id, _context);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            AppUser member = await _context.Users.SingleOrDefaultAsync(c => c.Email == memberEmail);
            if (member == null)
                return StatusCode(404, "user with this email not found");

            if (member == requestedUser)
                return BadRequest("you can not make yourself a member , you are an admin");


            if (existEvent.IsLimitedMember)
                if (existEvent.EventMembers.Count() >= existEvent.MaximumNumberOfMembers)
                    return BadRequest("Event is full");

            var userRoleInDb = await _context.EventUserRole.IgnoreQueryFilters()
                           .SingleOrDefaultAsync(x => x.AppUserId == member.Id && x.EventId == existEvent.Id);
            if (userRoleInDb == null)
            {
                var memberRole = new EventUserRole() { AppUser = member, AppUserId = member.Id, Event = existEvent, EventId = existEvent.Id, Role = Role.Member };
                await _context.EventUserRole.AddAsync(memberRole);
                existEvent.EventMembers.Add(member);
                await _context.SaveChangesAsync();
                return Ok("member added");
            }
            else if (userRoleInDb.IsDeleted)
            {
                _context.EventUserRole.Remove(userRoleInDb);//remove last information 
                await _context.SaveChangesAsync();
                return await PromoteMember(id, memberEmail);
            }
            else if (userRoleInDb.Role == Role.Admin)
            {
                userRoleInDb.Role = Role.Member;
                existEvent.EventMembers.Add(member);
                await _context.SaveChangesAsync();
                return Ok("this admin demoted to member");
            }
            else if (userRoleInDb.Role == Role.Member && userRoleInDb.Status == UserRoleStatus.Accepted)
            {
                return BadRequest("this user is already a member of this event");
            }
            else if (userRoleInDb.Role == Role.Member && userRoleInDb.Status != UserRoleStatus.Accepted)
            {
                existEvent.EventMembers.Add(member);
                userRoleInDb.Status = UserRoleStatus.Accepted;
                await _context.SaveChangesAsync();
                return Ok("user request accepted and this user is a member now");
            }

            return BadRequest();
        }

        [Authorize]
        [ServiceFilter(typeof(NotBlocked))]
        [HttpDelete]
        public async Task<ActionResult> KickUser(int id, string userEmail)
        {
            Event existEvent = await _context.Events.FindAsync(id);
            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            Role? userRole = await jwtHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], id, _context);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            AppUser user = await _context.Users.SingleOrDefaultAsync(c => c.Email == userEmail);
            if (user == null)
                return StatusCode(404, "user with this email not found");

            var userRoleInDb = await _context.EventUserRole.FindAsync(user.Id, id);
            if (userRoleInDb == null || userRoleInDb.IsDeleted || userRoleInDb.Status != UserRoleStatus.Accepted)
                return BadRequest("this user don't have any role in Event");

            if (userRoleInDb.Role == Role.Member)
                existEvent.EventMembers.Remove(user);

            userRoleInDb.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok("user kicked");
        }

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> Leave(int id)
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);

            Event existEvent = await _context.Events.FindAsync(id);
            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            var userRoleInDb = await _context.EventUserRole.FindAsync(requestedUser.Id, existEvent.Id);
            if (userRoleInDb == null || userRoleInDb.IsDeleted || userRoleInDb.Status != UserRoleStatus.Accepted)
                return BadRequest("you don't have any role in this Event");

            if (userRoleInDb.Role == Role.Member)
                existEvent.EventMembers.Remove(requestedUser);
            
            userRoleInDb.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok("you left the Event successfully");
        }

        [Authorize]
        [ServiceFilter(typeof(NotBlocked))]
        [HttpPatch]
        public async Task<ActionResult> PromoteAdmin(int id, string memberEmail)
        {
            Event existEvent = await _context.Events.FindAsync(id);
            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            Role? userRole = await jwtHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], id, _context);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            AppUser member = await _context.Users.SingleOrDefaultAsync(c => c.Email == memberEmail);
            if (member == null)
                return StatusCode(404, "Member not found");


            if (!existEvent.EventMembers.Contains(member))
                return StatusCode(403, "access denied, only members can be promote to admin");

            var userRoleInDb = await _context.EventUserRole.FindAsync(member.Id, existEvent.Id);
            userRoleInDb.Role = Role.Admin;
            existEvent.EventMembers.Remove(member);
            await _context.SaveChangesAsync();

            return Ok("member promoted");
        }


        [HttpPost]
        public async Task<ActionResult<ICollection<Event>>> Filter(FilterEventsViewModel model, [FromQuery] PaginationParameters pagination)
        {

            if ((model.DateMax != null && model.DateMin != null) && DateTime.Compare((DateTime)model.DateMin, (DateTime)model.DateMax) >= 0) return BadRequest("Date interval is negative");
            var FilteredEvents = await _eventService.FilterEvents(model, pagination);
            
            foreach (var ev in FilteredEvents)
            {
                if (ev.Images.Count > 0)
                {
                    foreach (var img in ev.Images)
                        img.ImageLink = minIOService.GenerateEventsUrl(img.FileName).Result;
                    await _context.SaveChangesAsync();
                }
            }

            List<OutputEventViewModel> outModels = new List<OutputEventViewModel>();
            foreach (var ev in FilteredEvents) outModels.Add(new OutputEventViewModel(ev));
            return Ok(outModels);
        }

        [HttpPost]
        public async Task<ActionResult<OutputEventViewModel>> Convert(int id)
        {
            AppUser user = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var ev = _context.Events.Find(id);
            if (ev == null) return NotFound("Event not found");
            if (!ev.IsProject) return BadRequest("Event type is not project");
            if (ev.Creator != user) return StatusCode(403, "only event creator can change the event type");
            ev.Tasks.Clear();
            ev.IsProject = false;
            ev.BuyingTicketEnabled = true;
            _context.SaveChanges();
            return (new OutputEventViewModel(ev));
        }


    }
}
