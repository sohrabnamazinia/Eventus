using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class EventClaim
    {
        public virtual AppUser AppUser { get; set; }
        public string AppUserId { get; set; }
        public virtual Event Event { get; set; }
        public int EventId { get; set; }

        public Role Role { get; set; }
    }
}
