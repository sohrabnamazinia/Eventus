using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using ArsamBackend.Models;
using ArsamBackend.Security;
using ArsamBackend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Task = ArsamBackend.Models.Task;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ArsamBackend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ILogger<TaskController> _logger;
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context, ILogger<TaskController> logger)
        {
            _logger = logger;
            this._context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Create(InputTaskViewModel incomeTask)
        {
            AppUser requestedUser = await JWTokenHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            Event taskEvent = await _context.Events.FindAsync(incomeTask.EventId);

            if (taskEvent == null)
                return StatusCode(404, "event not found");

            if (requestedUser != taskEvent.Creator)
                return StatusCode(403, "access denied");

            Task newTask = new Task()
            {
                Name = incomeTask.Name,
                Status = Status.Todo,
                Order = incomeTask.Order,
                Event = taskEvent,
                IsDeleted = false,
                AssignedMembers = new List<AppUser>()  
            };
            await _context.Tasks.AddAsync(newTask);
            await _context.SaveChangesAsync();

            var result = new OutputTaskViewModel(newTask);
            return Ok(result);
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> Update(int id, InputTaskViewModel incomeTask)
        {
            AppUser requestedUser = await JWTokenHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var existTask = await _context.Tasks.FindAsync(id);

            if (existTask == null || existTask.IsDeleted)
                return StatusCode(404, "task not found");

            Event taskEvent = existTask.Event;

            if (taskEvent == null)
                return StatusCode(404, "event not found");

            if (!(taskEvent.EventMembers.Contains(requestedUser) || requestedUser == taskEvent.Creator))
                return StatusCode(403, "access denied");

            existTask.Name = incomeTask.Name;
            existTask.Order = incomeTask.Order;
            existTask.Status = (Status)incomeTask.Status;

            await _context.SaveChangesAsync();

            var result = new OutputTaskViewModel(existTask);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            AppUser requestedUser = await JWTokenHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var existTask = await _context.Tasks.FindAsync(id);

            if (existTask == null || existTask.IsDeleted)
                return StatusCode(404, "task not found");

            Event taskEvent = existTask.Event;

            if (taskEvent == null)
                return StatusCode(404, "event not found");

            if (!(taskEvent.EventMembers.Contains(requestedUser) || requestedUser == taskEvent.Creator))
                return StatusCode(403, "access denied");

            existTask.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok("task deleted");
        }


        [Authorize]
        [HttpPut]
        public async Task<ActionResult> AssignMember(int id, string memberEmail)
        {
            AppUser requestedUser = await JWTokenHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            Task existTask = await _context.Tasks.FindAsync(id);

            if (existTask == null || existTask.IsDeleted)
                return StatusCode(404, "task not found");

            Event taskEvent = existTask.Event;

            if (taskEvent == null)
                return StatusCode(404, "event not found");

            if (!(taskEvent.EventMembers.Contains(requestedUser) || requestedUser == taskEvent.Creator))
                return StatusCode(403, "access denied");

            var member = await _context.Users.SingleOrDefaultAsync(c => c.Email == memberEmail);
            if (!existTask.AssignedMembers.Contains(member))
            {
                if (!existTask.Event.EventMembers.Contains(member))
                {
                    return StatusCode(403, "access denied, member must be eventMember");
                }
                var assignedMembersList = existTask.AssignedMembers.ToList();
                assignedMembersList.Add(member);
                existTask.AssignedMembers = assignedMembersList;
                existTask.Status = Status.Doing;

                await _context.SaveChangesAsync();

                var result = new OutputTaskViewModel(existTask);
                return Ok(result);
            }

            return BadRequest("member is already assigned");
        }
    }
}
