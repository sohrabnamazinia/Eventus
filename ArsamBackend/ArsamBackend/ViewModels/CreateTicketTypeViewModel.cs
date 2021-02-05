using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.ViewModels
{
    public class CreateTicketTypeViewModel
    {
        [Required]
        public int EventId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public long Price { get; set; }
        [Required]
        public long Capacity { get; set; }
    }
}
