using ArsamBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.ViewModels
{
    public class TicketOutputViewModel
    {
        public TicketOutputViewModel(Ticket ticket)
        {
            Id = ticket.Id;
            Event = ticket.EventId;
            Type = ticket.TicketTypeId;
            User = ticket.User.UserName;
        }

        public int Id { get; set; }
        public int Event { get; set; }
        public int? Type { get; set; }
        public string User { get; set; }

    }
}
