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
        public string Status { get; set; } 
        
        public int Order { get; set; } 

        [Required]
        public int EventId { get; set; }
       
        public bool IsDeleted { get; set; }

        public List<string> AssignedMembers { get; set; }
    }
}
