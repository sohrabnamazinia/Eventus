using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.ViewModels
{
    public class EventViewModel
    {
        [Required]
        public string Name { get; set; }

        public bool IsPrivate { get; set; }

        public string PasswordConfirmation { get; set; }

        public string Location { get; set; }


    }
}