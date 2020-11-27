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
            FirstName = user.FirstName;
            LastName = user.LastName;
            Description = user.Description;
            Fields = CategoryService.ConvertCategoriesToList(user.Fields);
            CreatedEvents = user.CreatedEvents.ToList().Select(x => new OutputAbstractViewModel(x)).ToList();
            InEvents = user.InEvents.Select(x => new OutputAbstractViewModel(x)).ToList();
        }
        
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public ICollection<int> Fields { get; set; }
        public ICollection<OutputAbstractViewModel> InEvents { get; set; }
        public ICollection<OutputAbstractViewModel> CreatedEvents { get; set; }

    }

    public class UpdateProfileViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public List<int> Fields { get; set; }
    }
}
