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
            Event eve = await _context.Events.FindAsync(eventId);

            if (requestedUser != eve.Creator)
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

                    var image = new Image()
                    {
                        EventId = eventId,
                        Event = eve,
                        FileName = fileName,
                        ContentType = file.ContentType
                    };
                    eve.Images.Add(image);
                    await _context.SaveChangesAsync();
                }
                else
                    return BadRequest("image not found");
            }
            var result = new OutputEventViewModel(eve);
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

            OutputEventViewModel result;

            if (userRole == Role.Admin || userRole == Role.Member)
            {
                result = new OutputEventViewModel(resultEvent, userRole);
                return Ok(result);
            }

            if (resultEvent.IsPrivate)
                return StatusCode(403, "access denied, this event is private");

            result = new OutputEventViewModel(resultEvent);
            return Ok(result);
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> Update(int id, InputEventViewModel incomeEvent)
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            Event existEvent = await _context.Events.FindAsync(id);

            if (existEvent == null || existEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            if (existEvent.Creator != requestedUser)
                return StatusCode(403, "access denied");

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
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            if (requestedUser == null)
                return StatusCode(401, "user not founded");

            Event existEvent = await _context.Events.SingleOrDefaultAsync(x => x.Id == id);

            if (existEvent == null || existEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            if (existEvent.Creator != requestedUser)
                return StatusCode(403, "access denied");

            existEvent.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok("event deleted");
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            if (requestedUser == null)
                return StatusCode(401, "user not founded");

            var events = await _context.Events.Where(x => !x.IsDeleted).ToListAsync();

            if (events == null)
                return NotFound("no event found");

            var result = new List<OutputEventViewModel>();
            foreach (var ev in events)
                result.Add(new OutputEventViewModel(ev));

            return Ok(result);
        }


        [Authorize]
        [HttpPut]
        public async Task<ActionResult> PromoteMember(int id, string memberEmail)
        {
            Role? userRole = jwtHandler.FindRoleByToken(Request.Headers[HeaderNames.Authorization], id);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            AppUser member = await _context.Users.SingleOrDefaultAsync(c => c.Email == memberEmail);
            if (member == null)
                return StatusCode(404, "user with this email not found");

            Event existEvent = await _context.Events.FindAsync(id);

            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            if (existEvent.IsLimitedMember)
                if (existEvent.EventMembers.Count() >= existEvent.MaximumNumberOfMembers)
                    return BadRequest("Event is full");

            var userRoleInDb = await _context.EventClaim.FindAsync(member.Id, existEvent.Id);
            if (userRoleInDb == null)
            {
                var memberRoleClaim = new EventClaim() { AppUser = member, AppUserId = member.Id, Event = existEvent, EventId = existEvent.Id, Role = Role.Member };
                await _context.EventClaim.AddAsync(memberRoleClaim);
                var membersList = existEvent.EventMembers.ToList();
                membersList.Add(member);
                existEvent.EventMembers = membersList;
                await _context.SaveChangesAsync();
                var result = new OutputEventViewModel(existEvent, userRole);
                return Ok(result);
            }
            else if (userRoleInDb.Role == Role.Admin)
            {
                userRoleInDb.Role = Role.Member;
                await _context.SaveChangesAsync();
                return Ok("this admin demoted to member");
            }
            else if (userRoleInDb.Role == Role.Member)
            {
                return BadRequest("member is already assigned");
            }

            return BadRequest();
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> PromoteAdmin(int id, string memberEmail)
        {
            Role? userRole = jwtHandler.FindRoleByToken(Request.Headers[HeaderNames.Authorization], id);
            if (userRole != Role.Admin)
                return StatusCode(403, "access denied, you are not an admin");

            AppUser member = await _context.Users.SingleOrDefaultAsync(c => c.Email == memberEmail);
            if (member == null)
                return StatusCode(404, "Member not found");

            Event existEvent = await _context.Events.FindAsync(id);

            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            if (!existEvent.EventMembers.Contains(member))
                return StatusCode(403, "access denied, only members can be promote to admin");

            var userRoleInDb = await _context.EventClaim.FindAsync(member.Id, existEvent.Id);
            userRoleInDb.Role = Role.Admin;
            await _context.SaveChangesAsync();

            return Ok("member promoted");
        }


        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ICollection<Event>>> Filter(FilterEventsViewModel model, [FromQuery] PaginationParameters pagination)
        {
            var FilteredEvents = await _eventService.FilterEvents(model, pagination);
            List<OutputEventViewModel> outModels = new List<OutputEventViewModel>();
            foreach (var ev in FilteredEvents) outModels.Add(new OutputEventViewModel(ev));
            return Ok(outModels);
        }



    }
}
