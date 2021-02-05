using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class EventUserRole
    {
        public virtual AppUser AppUser { get; set; }
        public string AppUserId { get; set; }
        public virtual Event Event { get; set; }
        public int EventId { get; set; }
        public Role Role { get; set; }
        public UserRoleStatus Status { get; set; } = UserRoleStatus.Accepted;
        public DateTime DateOfRequest { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
    }

    public enum UserRoleStatus
    {
        Accepted = 1,
        Pending = 0,
        Rejected = -1
    }
}
