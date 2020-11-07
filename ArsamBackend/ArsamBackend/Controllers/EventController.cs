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
using ArsamBackend.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Task = System.Threading.Tasks.Task;

namespace ArsamBackend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly AppDbContext _context;

        public EventController(AppDbContext context, ILogger<EventController> logger)
        {
            _logger = logger;
            this._context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Create(InputEventViewModel incomeEvent)
        {
            var requestedUserEmail = JWTokenHandler.FindEmailByToken(Request.Headers[HeaderNames.Authorization]);

            Event newEvent = new Event()
            {
                Name = incomeEvent.Name,
                IsProject = incomeEvent.IsProject,
                Description = incomeEvent.Description,
                IsPrivate = incomeEvent.IsPrivate,
                Location = "",
                StartDate = incomeEvent.StartDate,
                EndDate = incomeEvent.EndDate,
                IsLimitedMember = incomeEvent.IsLimitedMember,
                MaximumNumberOfMembers = incomeEvent.MaximumNumberOfMembers,
                EventMembersEmail = new List<string>(),
                CreatorEmail = requestedUserEmail,
                IsDeleted = false,
                ImagesFilePath = new List<string>()
            };

            await _context.Events.AddAsync(newEvent);
            await _context.SaveChangesAsync();

            var result = new OutputEventViewModel(newEvent);
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> AddImage(int eventId)
        {
            var requestedUserEmail = JWTokenHandler.FindEmailByToken(Request.Headers[HeaderNames.Authorization]);
            Event eve = await _context.Events.FindAsync(eventId);

            if (requestedUserEmail != eve.CreatorEmail)
                return StatusCode(403, "access denied");
            
            var req = Request.Form.Files;

            string path = ("Images/Events/");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (var file in req)
            {
                if (file != null && file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(file.FileName);
                    await using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    var imagesList = eve.ImagesFilePath.ToList();
                    imagesList.Add(Path.Combine(path, fileName));
                    eve.ImagesFilePath = imagesList;
                    await _context.SaveChangesAsync();
                }
                return BadRequest("image not found");
            }
            return Ok("images added");

        }

        //[Authorize]
        //[HttpGet]
        //public async Task<ActionResult<InputEventViewModel>> Get(int id)
        //{
        //    AppUser requestedUser = await FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization]);
        //    if (requestedUser == null)
        //        return StatusCode(401, "user not founded");

        //    var resultEvent = await _context.Events.SingleOrDefaultAsync(x => x.Id == id);
        //    if (resultEvent == null || resultEvent.IsDeleted)
        //        return NotFound("no event found by this id: " + id);


        //    if (resultEvent.CreatorEmail == requestedUser.Email) //creator
        //    {
        //        var result = new ActionResult<InputEventViewModel>(new InputEventViewModel()
        //        {
        //            Name = resultEvent.Name,
        //            id = id,
        //            IsPrivate = resultEvent.IsPrivate,
        //            Location = resultEvent.Location,
        //            CreatorEmail = resultEvent.CreatorEmail
        //        });
        //        return result;
        //    }
        //    else // everyone
        //    {
        //        var result = new ActionResult<InputEventViewModel>(new InputEventViewModel()
        //        {
        //            id = resultEvent.Id,
        //            Name = resultEvent.Name,
        //            Location = resultEvent.Location,
        //            IsPrivate = resultEvent.IsPrivate
        //        });
        //        return result;
        //    }
        //}

        //[Authorize]
        //[HttpPut]
        //public async Task<ActionResult<InputEventViewModel>> Update(int id, InputEventViewModel incomeInputEvent)
        //{
        //    AppUser requestedUser = await FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization]);
        //    if (requestedUser == null)
        //        return StatusCode(401, "user not founded");

        //    Event existEvent = await _context.Events.SingleOrDefaultAsync(x => x.Id == id);

        //    if (existEvent == null || existEvent.IsDeleted)
        //        return NotFound("no event found by this id: " + id);

        //    if (existEvent.CreatorEmail != requestedUser.Email)
        //        return StatusCode(403, "access denied");

        //    existEvent.Location = incomeInputEvent.Location;
        //    existEvent.IsPrivate = incomeInputEvent.IsPrivate;
        //    existEvent.Name = incomeInputEvent.Name;
        //    await _context.SaveChangesAsync();

        //    var result = new InputEventViewModel()
        //    {
        //        Name = existEvent.Name,
        //        id = existEvent.Id,
        //        IsPrivate = existEvent.IsPrivate,
        //        Location = existEvent.Location,
        //        CreatorEmail = existEvent.CreatorEmail
        //    };

        //    return Ok(result);
        //}

        //[Authorize]
        //[HttpDelete]
        //public async Task<ActionResult> Delete(int id)
        //{
        //    AppUser requestedUser = await FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization]);
        //    if (requestedUser == null)
        //        return StatusCode(401, "user not founded");

        //    Event existEvent = await _context.Events.SingleOrDefaultAsync(x => x.Id == id);

        //    if (existEvent == null || existEvent.IsDeleted)
        //        return NotFound("no event found by this id: " + id);

        //    if (existEvent.CreatorEmail != requestedUser.Email)
        //        return StatusCode(403, "access denied");

        //    existEvent.IsDeleted = true;
        //    await _context.SaveChangesAsync();
        //    return Ok("event deleted");
        //}

        //[Authorize]
        //[HttpGet]
        //public async Task<ActionResult<List<InputEventViewModel>>> GetAll()
        //{
        //    AppUser requestedUser = await FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization]);
        //    if (requestedUser == null)
        //        return StatusCode(401, "user not founded");

        //    var events = await _context.Events.Where(x => !x.IsDeleted).ToListAsync();
        //    if (events == null)
        //        return NotFound("no event found");
        //    var result = new List<InputEventViewModel>();
        //    foreach (var ev in events)
        //    {
        //        result.Add(new InputEventViewModel()
        //        {
        //            Name = ev.Name,
        //            Location = ev.Location,
        //            CreatorEmail = ev.CreatorEmail,
        //            id = ev.Id,
        //            IsPrivate = ev.IsPrivate
        //        });
        //    }
        //    return Ok(result);

        //}

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<OutputTaskViewModel>>> GetTasks(int id)
        {
            AppUser requestedUser = await FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization]);
            if (requestedUser == null)
                return StatusCode(401, "user not founded");

            var taskEvent = await _context.Events.SingleOrDefaultAsync(x => x.Id == id);
            if (taskEvent == null || taskEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            var tasks = _context.Tasks.Where(x => x.EventId == id).ToList();
            if (tasks.Count == 0)
                return NotFound("there is no task for this event");

            var result = new List<OutputTaskViewModel>();
            foreach (var task in tasks)
                result.Add(new OutputTaskViewModel(task.Id, task.Name, task.Status, task.Order, task.EventId, task.AssignedMembers));

            return result;
        }

        //methods
        [NonAction]
        private async Task<AppUser> FindUserByTokenAsync(string authorization)
        {
            string token = string.Empty;
            if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                var scheme = headerValue.Scheme;
                token = headerValue.Parameter;
            }
            var userEmail = JWTokenHandler.GetClaim(token, "nameid");
            return await _context.Users.SingleOrDefaultAsync(x => x.Email == userEmail);
        }
    }
}
