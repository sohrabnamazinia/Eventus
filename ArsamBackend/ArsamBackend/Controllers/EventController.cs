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
                        FileName =  fileName,
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
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);

            Event resultEvent = await _context.Events.FindAsync(id);

            if (resultEvent == null || resultEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            if (resultEvent.Creator == requestedUser) //creator
            {
                var resultForCreator = new OutputEventViewModel(resultEvent);
                return Ok(resultForCreator);
            }
            else if (resultEvent.EventMembers.Contains(requestedUser)) //members
            {
                var resultForMembers = new Output2EventViewModel(resultEvent);
                return Ok(resultForMembers);
            }

            if (resultEvent.IsPrivate)
                return StatusCode(403, "access denied");

            var resultForAll = new Output2EventViewModel(resultEvent);
            return Ok(resultForAll);
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
        public async Task<ActionResult> JoinMember(int id, string memberEmail)
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            Event existEvent = await _context.Events.FindAsync(id);

            if (existEvent == null || existEvent.IsDeleted)
                return StatusCode(404, "event not found");

            if (existEvent.Creator != requestedUser)
                return StatusCode(403, "access denied");

            var member = await _context.Users.SingleOrDefaultAsync(c => c.Email == memberEmail);

            if (member == null)
                return StatusCode(404, "Member not found");

            if (existEvent.IsLimitedMember)
                if (existEvent.EventMembers.Count() >= existEvent.MaximumNumberOfMembers)
                    return BadRequest("Event is full");
            
            if (!existEvent.EventMembers.Contains(member))
            {
                var membersList = existEvent.EventMembers.ToList();
                membersList.Add(member);
                existEvent.EventMembers = membersList;

                await _context.SaveChangesAsync();

                var result = new OutputEventViewModel(existEvent);
                return Ok(result);
            }

            return BadRequest("member is already assigned");
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
