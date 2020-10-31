using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using ArsamBackend.Models;
using ArsamBackend.Security;
using ArsamBackend.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

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
        [HttpPost]
        public async Task<ActionResult> Create(EventViewModel incomeEvent)
        {
            AppUser requestedUser = await FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization]);
            if (requestedUser == null)
                return StatusCode(401, "you have to login first");

            Event eva = new Event()
            {
                Name = incomeEvent.Name,
                IsPrivate = incomeEvent.IsPrivate,
                Location = incomeEvent.Location,
                CreatorAppUser = requestedUser,
                CreatorEmail = requestedUser.Email,
                IsDeleted = false
            };
            await _context.Events.AddAsync(eva);
            await _context.SaveChangesAsync();
            incomeEvent.id = eva.Id;
            incomeEvent.CreatorEmail = requestedUser.Email;
            return Ok(incomeEvent);
        }

        [HttpGet]
        public async Task<ActionResult<EventViewModel>> Get(int id)
        {
            AppUser requestedUser = await FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization]);
            if (requestedUser == null)
                return StatusCode(401, "user not founded");

            var resultEvent = await _context.Events.SingleOrDefaultAsync(x => x.Id == id);
            if (resultEvent == null || resultEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            
            if (resultEvent.CreatorAppUser == requestedUser) //creator
            {
                var result = new ActionResult<EventViewModel>(new EventViewModel()
                {
                    Name = resultEvent.Name,
                    id = id,
                    IsPrivate = resultEvent.IsPrivate,
                    Location = resultEvent.Location,
                    CreatorEmail = resultEvent.CreatorEmail
                });
                return result;
            }
            else // everyone
            {
                var result = new ActionResult<EventViewModel>(new EventViewModel()
                {
                    id = resultEvent.Id,
                    Name = resultEvent.Name,
                    Location = resultEvent.Location,
                    IsPrivate = resultEvent.IsPrivate
                });
                return result;
            }

        }

        [HttpPut]
        public async Task<ActionResult<EventViewModel>> Update(int id, EventViewModel incomeEvent)
        {
            AppUser requestedUser = await FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization]);
            if (requestedUser == null)
                return StatusCode(401,"user not founded");

            Event existEvent = await _context.Events.SingleOrDefaultAsync(x => x.Id == id);

            if (existEvent == null || existEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            if (existEvent.CreatorAppUser != requestedUser)
                return StatusCode(403, "access denied");

            existEvent.Location = incomeEvent.Location;
            existEvent.IsPrivate = incomeEvent.IsPrivate;
            existEvent.Name = incomeEvent.Name;
            await _context.SaveChangesAsync();

            var result = new EventViewModel()
            {
                Name = existEvent.Name,
                id = existEvent.Id,
                IsPrivate = existEvent.IsPrivate,
                Location = existEvent.Location,
                CreatorEmail = existEvent.CreatorEmail
            };

            return Ok(result);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            AppUser requestedUser = await FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization]);
            if (requestedUser == null)
                return StatusCode(401, "user not founded");


            Event existEvent = await _context.Events.SingleOrDefaultAsync(x => x.Id == id);

            if (existEvent == null || existEvent.IsDeleted)
                return NotFound("no event found by this id: " + id);

            if (existEvent.CreatorAppUser != requestedUser)
                return StatusCode(403, "access denied");


            existEvent.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok("event deleted");
        }

        [HttpGet]
        public async Task<ActionResult<List<EventViewModel>>> GetAll()
        {
            AppUser requestedUser = await FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization]);
            if (requestedUser == null)
                return StatusCode(401, "user not founded");

            var events = await _context.Events.Where(x => !x.IsDeleted).ToListAsync();
            if (events == null)
                return NotFound("no event found");
            var result = new List<EventViewModel>();
            foreach (var ev in events)
            {
                result.Add(new EventViewModel()
                {
                    Name = ev.Name,
                    Location = ev.Location,
                    CreatorEmail = ev.CreatorEmail,
                    id = ev.Id,
                    IsPrivate = ev.IsPrivate
                });
            }
            return Ok(result);

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
