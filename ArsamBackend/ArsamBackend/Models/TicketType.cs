using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class TicketType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual Event Event { get; set; }
        public int EventId { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public long Price { get; set; }
        [Required]
        public long Capacity { get; set; }
        public long Count { get; set; }

    }
    //Add-Migration Create-Table-TicketType
}
