using ArsamBackend.Models;
using ArsamBackend.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Services
{
    public interface IEventService
    {
        public Task<ICollection<Event>> FilterEvents(FilterEventsViewModel model);
    }
}
