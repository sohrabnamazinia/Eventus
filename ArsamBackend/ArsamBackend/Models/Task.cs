using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class Task
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public Status Status { get; set; }

        public int Order { get; set; }

        [Required]
        public virtual Event Event { get; set; }
        public int EventId { get; set; }

        public bool IsDeleted { get; set; }

        public virtual ICollection<AppUser> AssignedMembers { get; set; }
    }

    public enum Status
    {
        Todo = 1,
        Doing = 2,
        Done = 3
    }
}
