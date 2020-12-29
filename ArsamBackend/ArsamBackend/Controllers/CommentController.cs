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
    public class CommentController : ControllerBase
    {
        private readonly ILogger<CommentController> _logger;
        private readonly IEventService _eventService;
        private readonly IJWTService jwtHandler;
        private readonly IMinIOService minIOService;
        private readonly AppDbContext _context;

        public CommentController(IJWTService jwtHandler, AppDbContext context, ILogger<CommentController> logger, IEventService eventService, IMinIOService minIO)
        {
            _logger = logger;
            this._eventService = eventService;
            this.jwtHandler = jwtHandler;
            this.minIOService = minIO;
            this._context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ICollection<CommentOutputViewModel>>> Add(CommentInputViewModel comment)
        {
            AppUser requestedUser = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            if (requestedUser == null)
                return StatusCode(401, "token is invalid, user not found");

            Event ev = await _context.Events.FindAsync(comment.EventId);
            if (ev == null)
                return StatusCode(404, "event not found");

            Comment parent = await _context.Comments.FindAsync(comment.ParentId);
            if (parent != null && !ev.Comments.Contains(parent))
                return BadRequest("parent is not valid");
            
            var newComment = new Comment()
            {
                User = requestedUser,
                Event = ev,
                Parent = parent,
                DateTime = DateTime.Now,
                Description = comment.Description,
                Childs = new List<Comment>()
            };

            await _context.Comments.AddAsync(newComment);
            await _context.SaveChangesAsync();

            return await Get(comment.EventId);
        }

        [HttpGet]
        public async Task<ActionResult<ICollection<CommentOutputViewModel>>> Get(int eventId)
        {
           
            Event ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return StatusCode(404, "event not found");

            foreach (var comment in ev.Comments)
            {
                var user = comment.User;
                if (user.ImageName != null) user.ImageLink = minIOService.GenerateUrl(user.Id, user.ImageName).Result;
            }
            await _context.SaveChangesAsync();

            var result = ev.Comments.Where(x => x.Parent == null).OrderBy(x => x.DateTime).Select(x => new CommentOutputViewModel(x));
            return Ok(result);
        }

    }
}
