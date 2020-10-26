using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArsamBackend.Models;
using ArsamBackend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        public int Create(EventViewModel incomeEvent)
        {
            Event eva = new Event()
            {
                Name = incomeEvent.Name,
                IsPrivate = incomeEvent.IsPrivate,
                Location = incomeEvent.Location
            };
            _context.Events.Add(eva);
            _context.SaveChanges();
            return eva.Id;
        }
    }
}
