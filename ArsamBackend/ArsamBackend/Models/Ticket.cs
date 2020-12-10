using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public virtual Event Event { get; set; }
        public int EventId { get; set; }
        #nullable enable
        public virtual TicketType? Type { get; set; }
        public int? TicketTypeId { get; set; }
        #nullable disable
        public virtual AppUser User { get; set; }
        public string UserId { get; set; }
    }
}
