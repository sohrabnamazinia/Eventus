using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.ViewModels
{
    public class SendEmailConfirmationViewModel
    {
        public string FirstName;
        public string Email;
        public string Username;
        public string Password;
        public string ConfirmationLink;
    }
}
