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
        public virtual ICollection<Event> InEvents { get; set; }
        public virtual ICollection<Task> AssignedTasks { get; set; }
    }
}
