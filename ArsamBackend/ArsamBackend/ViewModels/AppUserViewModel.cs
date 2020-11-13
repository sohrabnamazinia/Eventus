using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArsamBackend.Models;

namespace ArsamBackend.ViewModels
{
    public class OutputAppUserViewModel
    {
        public OutputAppUserViewModel(AppUser user)
        {
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
        }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

    }
}
