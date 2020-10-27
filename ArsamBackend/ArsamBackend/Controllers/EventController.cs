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

        [HttpGet]
        public Event Get(int id)
        {
           return _context.Events.SingleOrDefault(x => x.Id == id);
        }

        [HttpPost]
        public bool Update(int id, EventViewModel incomeEvent)
        {
            Event existEvent = _context.Events.SingleOrDefault(x => x.Id == id);
            if (existEvent == null)
                return false;
            existEvent.Location = incomeEvent.Location;
            existEvent.IsPrivate = incomeEvent.IsPrivate;
            existEvent.Name = incomeEvent.Name;
            _context.SaveChanges();
            return true;
        }

        [HttpGet]
        public bool Delete(int id)
        {
            Event existEvent = _context.Events.SingleOrDefault(x => x.Id == id);
            if (existEvent == null)
                return false;
            _context.Events.Remove(existEvent);
            _context.SaveChanges();
            return true;
        }
    }
}
