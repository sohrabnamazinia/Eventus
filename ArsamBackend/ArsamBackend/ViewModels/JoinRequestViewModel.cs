using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArsamBackend.Models;

namespace ArsamBackend.ViewModels
{
    public class OutputJoinRequestViewModel
    {
        public OutputJoinRequestViewModel(EventUserRole userRole)
        {
            this.DateOfRequest = userRole.DateOfRequest;
            this.User = new EventOutputAppUserViewModel(userRole.AppUser);
        }
        public EventOutputAppUserViewModel User { get; set; }

        public DateTime DateOfRequest { get; set; }
    }
}
