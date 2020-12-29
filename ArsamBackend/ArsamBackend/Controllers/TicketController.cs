using ArsamBackend.Models;
using ArsamBackend.Services;
using ArsamBackend.Utilities;
using ArsamBackend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ILogger<AccountController> _logger;
        public readonly JwtSecurityTokenHandler handler;
        private readonly IDataProtector protector;
        private readonly IJWTService _jWTHandler;
        private readonly IEmailService emailService;

        public TicketController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AccountController> logger, IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurposeStrings, IJWTService jWTHandler, IEmailService emailService)
        {
            this._context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._logger = logger;
            this.protector = dataProtectionProvider.CreateProtector(DataProtectionPurposeStrings.UserIdQueryString);
            this._jWTHandler = jWTHandler;
            this.emailService = emailService;
        }

        [HttpPost]
        public async Task<ActionResult<TicketType>> CreateType(CreateTicketTypeViewModel model)
        {
            AppUser requestedUser = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var ev = _context.Events.Find(model.EventId);
            if (ev == null) return NotFound("Event not found!");
            if (ev.IsBlocked) return StatusCode(402, "Upgrade To Premium");
            var requestedUserRole = await _jWTHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], ev.Id, _context);
            if (!(requestedUserRole == Role.Admin)) return StatusCode(403, "Access Denied");
            if (model.Capacity <= 0 || model.Price < 0) return BadRequest("Capacity must be positive!");

            var type = new TicketType()
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Event = ev,
                Capacity = model.Capacity,
                Tickets = new List<Ticket>()
            };

            _context.TicketTypes.Add(type);
            _context.SaveChanges();

            return Ok(new TicketTypeOutputViewModel(type));
        }

        [HttpPost]
        public async Task<ActionResult<Ticket>> CreateTicket(CreateTicketViewModel model)
        {
            AppUser user = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var tt = _context.TicketTypes.Find(model.TypeId);
            if (tt == null) return NotFound("Ticket type not found!");
            var ev = tt.Event;
            if (ev.IsBlocked) return StatusCode(402, "Upgrade To Premium");
            if (ev.Tickets.Count >= 50) return StatusCode(402, "Upgrade To Premium");
            //var user = await userManager.FindByEmailAsync(model.UserEmail);
            //if (requestedUser != user) return Conflict("users can not buy tickets for others!");
            if (ev == null) return NotFound("Event not found!");
            if (user == null) return NotFound("User not found!");
            if (!ev.BuyingTicketEnabled) return StatusCode(403, "Currently, this event does not sell tickets!");
            if (user.Balance < tt.Price) return StatusCode(403, "balance not sufficient");
            user.Balance -= tt.Price;
            var t = new Ticket();
            t.Event = ev;
            t.User = user;
            if (tt.Count == tt.Capacity) return BadRequest("All tickets of this type are sold!");
            t.Type = tt;
            tt.Count++;
            _context.Tickets.Add(t);
            _context.SaveChanges();
            emailService.SendTicketNotification(new SendTicketNotificationViewModel 
            {
                Email = user.Email,
                EventName = t.Event.Name,
                Username = user.UserName,
                TicketTypeName = t.Type.Name,
                price = t.Type.Price,
                balance = user.Balance,
                FirstName = user.FirstName
            });
            return Ok(new TicketOutputViewModel(t));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTicket([FromBody] int tId)
        {
            AppUser requestedUser = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var t = _context.Tickets.Find(tId);
            if (t.Event.IsBlocked) return StatusCode(402, "Upgrade To Premium");
            if (t == null) return NotFound("Ticket is not found!");
            if (t.User != requestedUser) return StatusCode(403, "deleting a ticket is just allowed for the owner of that!");
            _context.Tickets.Remove(t);
            t.Type.Count--;
            _context.SaveChanges();
            return Ok("Ticket is removed!");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteType([FromBody] int ttId)
        {
            AppUser requestedUser = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var tt = _context.TicketTypes.Find(ttId);
            if (tt == null) return NotFound("Ticket type not found!");
            var ev = tt.Event;
            if (ev.IsBlocked) return StatusCode(402, "Upgrade To Premium");
            var requestedUserRole = await _jWTHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], ev.Id, _context);
            if (!(requestedUserRole == Role.Admin)) return StatusCode(403, "Access Denied");
            if (tt.Tickets.Count != 0) return BadRequest("There are tickets of this type, you must first remove them!");
            _context.TicketTypes.Remove(tt);
            _context.SaveChanges();
            return Ok("The Ticket Type is removed!");
        }

        [HttpPut]
        public async Task<IActionResult> UpdateType(UpdateTicketTypeViewModel model)
        {
            AppUser requestedUser = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var tt = _context.TicketTypes.Find(model.Id);
            if (tt == null) return NotFound("Ticket type not found!");
            var ev = tt.Event;
            if (ev.IsBlocked) return StatusCode(402, "Upgrade To Premium");
            var requestedUserRole = await _jWTHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], ev.Id, _context);
            if (!(requestedUserRole == Role.Admin)) return StatusCode(403, "Access Denied");
            if (model.Capacity != null && model.Capacity <= 0) return BadRequest("Capacity must be positive!");
            if (model.Price != null && model.Price < 0) return BadRequest("Price must be positive!");
            tt.Capacity = model.Capacity == null ? tt.Capacity : (long) model.Capacity;
            tt.Price = model.Price == null ? tt.Price : (long) model.Price;
            tt.Description = model.Description;
            tt.Name = model.Name;
            _context.TicketTypes.Update(tt);
            _context.SaveChanges();
            return Ok(new TicketTypeOutputViewModel(tt));
        }

        [HttpGet]
        public async Task<ActionResult<List<TicketOutputViewModel>>> GetTicketTypeTickets(int ttId)
        {
            AppUser requestedUser = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var tt = _context.TicketTypes.Find(ttId);
            if (tt == null) return NotFound("Ticket type not found!");
            var ev = tt.Event;
            var requestedUserRole = await _jWTHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], ev.Id, _context);
            if (!(requestedUserRole == Role.Admin)) return StatusCode(403, "Access Denied");
            return tt.Tickets.Select(x => new TicketOutputViewModel(x)).ToList();
        }

        [HttpGet]
        public async Task<ActionResult<List<TicketOutputViewModel>>> GetEventTickets(int id)
        {
            AppUser requestedUser = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var ev = _context.Events.Find(id);
            if (ev == null) return NotFound("Ticket type not found!");
            var requestedUserRole = await _jWTHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], ev.Id, _context);
            if (!(requestedUserRole == Role.Admin)) return StatusCode(403, "Access Denied");
            return ev.Tickets.Select(x => new TicketOutputViewModel(x)).ToList();
        }

        [HttpGet]
        public async Task<ActionResult<List<TicketOutputViewModel>>> GetUserTickets(string Email)
        {
            AppUser requestedUser = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var user = await userManager.FindByEmailAsync(Email);
            if (user == null) return NotFound("User not found!");
            if (user != requestedUser) return StatusCode(403, "user's tickets is only visible to themselves!");
            return user.Tickets.Select(x => new TicketOutputViewModel(x)).ToList();
        } 

        [HttpGet]
        public async Task<ActionResult<List<TicketTypeOutputViewModel>>> GetEventTicketTypes(int id)
        {
            AppUser requestedUser = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var ev = _context.Events.Find(id);
            if (ev == null) return NotFound("Event not found!");
            var requestedUserRole = await _jWTHandler.FindRoleByTokenAsync(Request.Headers[HeaderNames.Authorization], ev.Id, _context);
            if (!(requestedUserRole == Role.Admin) && ev.IsPrivate) return StatusCode(403, "Access Denied");
            return ev.TicketTypes.Select(x => new TicketTypeOutputViewModel(x)).ToList();
        }
    }
}
