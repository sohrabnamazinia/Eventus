using ArsamBackend.Services;
using ArsamBackend.ViewModels;
using Google.Apis.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;
        public readonly ILogger<EventService> _logger;
        public EventService(AppDbContext context, ILogger<EventService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ICollection<Event>> FilterEvents(FilterEventsViewModel model)
        {
            var events = _context.Events.Include(x => x.Images).Where(x =>
            (model.Name == null || x.Name == model.Name) &&
            (model.IsPrivate == null || x.IsPrivate == model.IsPrivate) &&
            (model.MembersCountMin == null || x.EventMembers.Count() >= model.MembersCountMin) &&
            (model.MembersCountMax == null || x.EventMembers.Count() <= model.MembersCountMax) &&
            (model.DateMin == null || DateTime.Compare(x.StartDate, (DateTime)model.DateMin) >= 0) &&
            (model.DateMax == null || DateTime.Compare(x.EndDate, (DateTime)model.DateMax) <= 0));
            return await events.ToListAsync();
        }
    }
}
