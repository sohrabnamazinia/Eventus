using ArsamBackend.Models;
using ArsamBackend.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArsamBackend.Utilities;

namespace ArsamBackend.Services
{
    public interface IEventService
    {
        Task<Event> CreateEvent(InputEventViewModel incomeEvent, AppUser creator);
        Task<ICollection<Event>> FilterEvents(FilterEventsViewModel model, PaginationParameters pagination);
    }
}
