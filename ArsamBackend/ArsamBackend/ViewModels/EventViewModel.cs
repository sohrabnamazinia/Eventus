using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.ViewModels
{
   
    public class EventViewModel
    {
        [Required]
        public string Name { get; set; }
        public int id { get; set; }
        public string Location { get; set; }
        public string CreatorEmail { get; set; }
        public bool IsPrivate { get; set; }

    }
}