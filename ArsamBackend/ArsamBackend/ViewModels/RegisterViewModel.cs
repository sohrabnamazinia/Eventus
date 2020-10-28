using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.ViewModels
{
    public class RegisterViewModel
    {
        [EmailAddress]
        [Required]
        public string EmailAddress { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string PasswordConfirmation { get; set; }


    }
}
