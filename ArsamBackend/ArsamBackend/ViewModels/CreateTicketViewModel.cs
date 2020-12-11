using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.ViewModels
{
    public class CreateTicketViewModel
    {
        [Required]
        public int TypeId { get; set; }
        [Required]
        public string UserEmail { get; set; }
    }
}
