using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
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
        private readonly IJWTService jwtHandler;
        private readonly AppDbContext _context;

        public EventController(IJWTService jwtHandler, AppDbContext context, ILogger<EventController> logger, IEventService eventService)
        {
            _logger = logger;
            this._eventService = eventService;
            this.jwtHandler = jwtHandler;
            this._context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Create(InputEventViewModel incomeEvent)
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);

            Event createdEvent = await _eventService.CreateEvent(incomeEvent, requestedUser);

            var result = new OutputEventViewModel(createdEvent);
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> AddImage(int eventId)
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            Role? userRole = jwtHandler.FindRoleByToken(Request.Headers[HeaderNames.Authorization], eventId);

            Event existEvent = await _context.Events.FindAsync(eventId);
            if (existEvent == null || existEvent.IsDeleted)
                return NotFound("no event found by this id: " + eventId);

            if (!(requestedUser == existEvent.Creator || userRole == Role.Admin))
                return StatusCode(403, "access denied");

            var req = Request.Form.Files;


            string path = Constants.EventImagesPath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (var file in req)
            {
                if (file != null && file.Length > 0)
                {
                    if (!file.ContentType.ToLower().Contains("image"))
                        return StatusCode(415, "Unsupported Media Type, only image types can be added to Events");

                    var fileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(file.FileName);
                    await using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    var image = new EventImage()
                    {
                        EventId = eventId,
                        Event = existEvent,
                        FileName = fileName,
                        ContentType = file.ContentType
                    };
                    existEvent.Images.Add(image);
                    await _context.SaveChangesAsync();
                }
                else
                    return BadRequest("image not found");
            }
            var result = new OutputEventViewModel(existEvent);
            return Ok(result);

        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Get(int id)
        {
            Event resultEvent = await _context.Events.FindAsync(id);
            if (resultEvent == null || resultEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            Role? userRole = jwtHandler.FindRoleByToken(Request.Headers[HeaderNames.Authorization], id);

            if (resultEvent.IsPrivate && userRole == null)
                return StatusCode(403, "access denied, this event is private");

            if (userRole == Role.Admin)
            {
                List<AppUser> admins = _context.EventUserRole.Where(x => x.EventId == id && x.Role == Role.Admin)
                    .Select(x => x.AppUser).ToList();
                AdminOutputEventViewModel adminResult = new AdminOutputEventViewModel(resultEvent, admins, userRole);
                return Ok(adminResult);
            }

            OutputEventViewModel result = new OutputEventViewModel(resultEvent, userRole);
            return Ok(result);
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> Update(int id, InputEventViewModel incomeEvent)
        {
            Event existEvent = await _context.Events.FindAsync(id);
            if (existEvent == null || existEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            Role? userRole = jwtHandler.FindRoleByToken(Request.Headers[HeaderNames.Authorization], id);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            existEvent.Name = incomeEvent.Name;
            existEvent.IsProject = incomeEvent.IsProject;
            existEvent.Description = incomeEvent.Description;
            existEvent.IsPrivate = incomeEvent.IsPrivate;
            existEvent.StartDate = incomeEvent.StartDate;
            existEvent.EndDate = incomeEvent.EndDate;
            existEvent.IsLimitedMember = incomeEvent.IsLimitedMember;
            existEvent.MaximumNumberOfMembers = incomeEvent.MaximumNumberOfMembers;
            existEvent.Categories = CategoryService.BitWiseOr(incomeEvent.Categories);

            await _context.SaveChangesAsync();

            var result = new OutputEventViewModel(existEvent);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            
            Event existEvent = await _context.Events.SingleOrDefaultAsync(x => x.Id == id);
            if (existEvent == null || existEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            Role? userRole = jwtHandler.FindRoleByToken(Request.Headers[HeaderNames.Authorization], id);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            existEvent.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok("event deleted");
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> JoinRequest(int eventId)
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);

            Event existEvent = await _context.Events.FindAsync(eventId);

            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            Role? userRole = jwtHandler.FindRoleByToken(Request.Headers[HeaderNames.Authorization], eventId);
            if (userRole != null)
                return BadRequest("you are in this event already");

            var userRoleInDb = await _context.EventUserRole.FindAsync(requestedUser.Id, existEvent.Id);
            if (userRoleInDb == null)
            {
                var memberRoleRequest = new EventUserRole() { AppUser = requestedUser, AppUserId = requestedUser.Id, Event = existEvent, EventId = existEvent.Id, Role = Role.Member, IsAccepted = false };
                await _context.EventUserRole.AddAsync(memberRoleRequest);
                await _context.SaveChangesAsync();
                return Ok("your request has been sent");
            }
            if (!userRoleInDb.IsAccepted)
                return BadRequest("your join request has been sent before");
            else
                return BadRequest("you are a member , login again please");
        }


        [Authorize]
        [HttpPatch]
        public async Task<ActionResult> AcceptOrRejectJoinRequest(int eventId, string memberEmail, bool accept)
        {
            Event existEvent = await _context.Events.FindAsync(eventId);
            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            Role? userRole = jwtHandler.FindRoleByToken(Request.Headers[HeaderNames.Authorization], eventId);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            AppUser member = await _context.Users.SingleOrDefaultAsync(c => c.Email == memberEmail);
            if (member == null)
                return StatusCode(404, "user with this email not found");

            var userRoleInDb = await _context.EventUserRole.FindAsync(member.Id, eventId);
            if (userRoleInDb == null)
                return BadRequest("this user has no join request");

            if (userRoleInDb.IsAccepted)
                return BadRequest("this user is already accepted");

            if (!accept)
            {
                _context.EventUserRole.Remove(userRoleInDb);
                await _context.SaveChangesAsync();
                return Ok("user joinRequest rejected successfully");
            }

            userRoleInDb.IsAccepted = true;
            var membersList = existEvent.EventMembers.ToList();
            membersList.Add(member);
            existEvent.EventMembers = membersList;
            await _context.SaveChangesAsync();
            return Ok("accepted, this user is a member now");
        }

        [Authorize]
        [HttpPatch]
        public async Task<ActionResult> PromoteMember(int id, string memberEmail)
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);

            Event existEvent = await _context.Events.FindAsync(id);

            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            Role? userRole = jwtHandler.FindRoleByToken(Request.Headers[HeaderNames.Authorization], id);
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

            var userRoleInDb = await _context.EventUserRole.FindAsync(member.Id, existEvent.Id);
            if (userRoleInDb == null)
            {
                var memberRoleClaim = new EventUserRole() { AppUser = member, AppUserId = member.Id, Event = existEvent, EventId = existEvent.Id, Role = Role.Member };
                await _context.EventUserRole.AddAsync(memberRoleClaim);
                var membersList = existEvent.EventMembers.ToList();
                membersList.Add(member);
                existEvent.EventMembers = membersList;
                await _context.SaveChangesAsync();
                return Ok("member added");
            }
            else if (userRoleInDb.Role == Role.Admin)
            {
                userRoleInDb.Role = Role.Member;
                var membersList = existEvent.EventMembers.ToList();
                membersList.Add(member);
                existEvent.EventMembers = membersList;
                await _context.SaveChangesAsync();
                return Ok("this admin demoted to member");
            }
            else if (userRoleInDb.Role == Role.Member)
            {
                return BadRequest("this user is already a member of this event");
            }

            return BadRequest();
        }

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> KickUser(int id, string userEmail)
        {
            Event existEvent = await _context.Events.FindAsync(id);
            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            Role? userRole = jwtHandler.FindRoleByToken(Request.Headers[HeaderNames.Authorization], id);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            AppUser user = await _context.Users.SingleOrDefaultAsync(c => c.Email == userEmail);
            if (user == null)
                return StatusCode(404, "user with this email not found");

            var userRoleInDb = await _context.EventUserRole.FindAsync(user.Id, id);
            if (userRoleInDb == null)
                return BadRequest("this user don't have any role in Event");

            if (userRoleInDb.Role == Role.Member)
            {
                var membersList = existEvent.EventMembers.ToList();
                membersList.Remove(user);
                existEvent.EventMembers = membersList;
            }

            _context.EventUserRole.Remove(userRoleInDb);
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
            if (userRoleInDb == null)
                return BadRequest("you don't have any role in this Event");

            if (userRoleInDb.Role == Role.Member)
            {
                var membersList = existEvent.EventMembers.ToList();
                membersList.Remove(requestedUser);
                existEvent.EventMembers = membersList;
            }
            _context.EventUserRole.Remove(userRoleInDb);
            await _context.SaveChangesAsync();
            return Ok("you left the Event successfully");
        }

        [Authorize]
        [HttpPatch]
        public async Task<ActionResult> PromoteAdmin(int id, string memberEmail)
        {
            Event existEvent = await _context.Events.FindAsync(id);
            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            Role? userRole = jwtHandler.FindRoleByToken(Request.Headers[HeaderNames.Authorization], id);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            AppUser member = await _context.Users.SingleOrDefaultAsync(c => c.Email == memberEmail);
            if (member == null)
                return StatusCode(404, "Member not found");

           
            if (!existEvent.EventMembers.Contains(member))
                return StatusCode(403, "access denied, only members can be promote to admin");

            var userRoleInDb = await _context.EventUserRole.FindAsync(member.Id, existEvent.Id);
            userRoleInDb.Role = Role.Admin;
            var membersList = existEvent.EventMembers.ToList();
            membersList.Remove(member);
            existEvent.EventMembers = membersList;
            await _context.SaveChangesAsync();

            return Ok("member promoted");
        }


        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ICollection<Event>>> Filter(FilterEventsViewModel model, [FromQuery] PaginationParameters pagination)
        {
            
            if ((model.DateMax != null && model.DateMin != null) && DateTime.Compare((DateTime) model.DateMin, (DateTime) model.DateMax) >= 0) return BadRequest("Date interval is negative");
            var FilteredEvents = await _eventService.FilterEvents(model, pagination);
            List<OutputEventViewModel> outModels = new List<OutputEventViewModel>();
            foreach (var ev in FilteredEvents) outModels.Add(new OutputEventViewModel(ev));
            return Ok(outModels);
        }

    }
}
