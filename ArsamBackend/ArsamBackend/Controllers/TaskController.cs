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
            var userEmail = JWTokenHandler.FindEmailByToken(Request.Headers[HeaderNames.Authorization]);
            Event taskEvent = await _context.Events.FindAsync(incomeTask.EventId);

            if (taskEvent == null)
                return StatusCode(404, "event not found");

            if (userEmail != taskEvent.CreatorEmail)
                return StatusCode(403, "access denied");

            Task newTask = new Task()
            {
                Name = incomeTask.Name,
                Description = incomeTask.Description,
                Status = "todo",
                Order = incomeTask.Order,
                EventId = taskEvent.Id,
                AssignedMembers = new List<string>()  
            };
            await _context.Tasks.AddAsync(newTask);
            await _context.SaveChangesAsync();

            var result = new OutputTaskViewModel(newTask.Id, newTask.Name, newTask.Description,
                newTask.Status, newTask.Order, newTask.EventId, newTask.AssignedMembers);
            return Ok(result);
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> Update(int id, InputTaskViewModel incomeTask)
        {
            var userEmail = JWTokenHandler.FindEmailByToken(Request.Headers[HeaderNames.Authorization]);
            var existTask = await _context.Tasks.FindAsync(id);

            Event taskEvent = await _context.Events.FindAsync(existTask.EventId);

            if (taskEvent == null)
                return StatusCode(404, "event not found");

            if (!hasAccess(userEmail, existTask, taskEvent))
                return StatusCode(403, "access denied");

            existTask.Name = incomeTask.Name;
            existTask.Description = incomeTask.Name;
            //existTask.Status = incomeTask.Status;
            existTask.Order = incomeTask.Order;

            await _context.SaveChangesAsync();

            var result = new OutputTaskViewModel(existTask.Id, existTask.Name, existTask.Description,
                existTask.Status, existTask.Order, existTask.EventId, existTask.AssignedMembers);
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UploadTest()
        {
            var req = Request.Form.Files;
            foreach (var file in req)
            {
                if (file != null && file.Length > 0)
                {
                    var uploads = "C:\\Users\\alifa\\Desktop\\uploadTest";
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(file.FileName);
                        using (var fileStream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        var x = PhysicalFile(Path.Combine("C:\\Users\\alifa\\Desktop\\uploadTest", fileName), "image/png");

                        return BadRequest(x);
                    }
                }
            }
            return BadRequest("member is already assigned");
        }
        [Authorize]
        [HttpPut]
        public async Task<ActionResult> AssignMember(int id, string memberEmail)
        {
            var userEmail = JWTokenHandler.FindEmailByToken(Request.Headers[HeaderNames.Authorization]);
            var existTask = await _context.Tasks.FindAsync(id);

            Event taskEvent = await _context.Events.FindAsync(existTask.EventId);

            if (taskEvent == null)
                return StatusCode(404, "event not found");

            if (!hasAccess(userEmail, existTask, taskEvent))
                return StatusCode(403, "access denied");

            if (!existTask.AssignedMembers.Contains(memberEmail))
            {
                var assignedMembersList = existTask.AssignedMembers.ToList();
                assignedMembersList.Add(memberEmail);
                existTask.AssignedMembers = assignedMembersList;
                await _context.SaveChangesAsync();
                return Ok(existTask.AssignedMembers);
            }

            return BadRequest("member is already assigned");
        }


        [NonAction]
        private bool hasAccess(string userEmail, Task existTask, Event taskEvent)
        {
            if (existTask.Status == "todo")
            {
                if (userEmail == taskEvent.CreatorEmail)
                    return true;
            }
            else if (existTask.Status == "doing")
            {
                foreach (var assignedMemberEmail in existTask.AssignedMembers)
                    if (assignedMemberEmail == userEmail)
                        return true;
            }

            return false;
        }
    }
}
