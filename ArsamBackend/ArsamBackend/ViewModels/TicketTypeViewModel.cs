using ArsamBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.ViewModels
{
    public class TicketTypeOutputViewModel
    {
        public TicketTypeOutputViewModel(TicketType type)
        {
            Id = type.ID;
            Name = type.Name;
            Description = type.Description;
            Event = new OutputAbstractViewModel(type.Event);
            Price = type.Price;
            Count = type.Count;
            Capacity = type.Capacity;
            Tickets = type.Tickets.Select(x => new TicketOutputViewModel(x)).ToList();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public OutputAbstractViewModel Event { get; set; }
        public long Price { get; set; }
        public long Count { get; set; }
        public long Capacity { get; set; }
        public List<TicketOutputViewModel> Tickets { get; set; }

    }
}
