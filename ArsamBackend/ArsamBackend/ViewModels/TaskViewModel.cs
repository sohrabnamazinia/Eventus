using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ArsamBackend.Models;
using Task = ArsamBackend.Models.Task;

namespace ArsamBackend.ViewModels
{
    public class InputTaskViewModel
    {
        [Required]
        public string Name { get; set; }

        public int Status { get; set; }

        public int Order { get; set; }

        [Required]
        public int EventId { get; set; }

    }

    public class OutputTaskViewModel
    {
        public OutputTaskViewModel(Task task)
        {
            Id = task.Id;
            Name = task.Name;
            Status = (int)task.Status;
            Order = task.Order;
            EventId = task.Event.Id;
            AssignedMembers = task.AssignedMembers.Select(x => new OutputAppUserViewModel(x)).ToList();
        }
        public int Id { get; set; }

        public string Name { get; set; }

        public int Status { get; set; }

        public int Order { get; set; }

        public int EventId { get; set; }

        public List<OutputAppUserViewModel> AssignedMembers { get; set; }

    }

}
