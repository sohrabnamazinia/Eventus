using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ArsamBackend.ViewModels
{
    public class InputTaskViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public int Order { get; set; }

        [Required]
        public int EventId { get; set; }

    }

    public class OutputTaskViewModel
    {
        public OutputTaskViewModel(int id, string name, string description, string status, int order, int eventId, List<string> assignedMembers)
        {
            Id = id;
            Name = name;
            Description = description;
            Status = status;
            Order = order;
            EventId = eventId;
            AssignedMembers = assignedMembers;
        }
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public int Order { get; set; }

        public int EventId { get; set; }

        public List<string> AssignedMembers { get; set; }

    }

}
