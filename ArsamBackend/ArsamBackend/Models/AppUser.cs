using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class AppUser : IdentityUser
    {
        public virtual ICollection<Event> CreatedEvents { get; set; }
        public virtual ICollection<EventUserRole> Roles { get; set; }
        public virtual ICollection<Event> InEvents { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<Task> AssignedTasks { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public virtual UserImage Image { get; set; }
        public string ImageName { get; set; }
        public string ImageLink { get; set; }
        public virtual Category Fields { get; set; }
        public long Balance { get; set; }

    }
}
