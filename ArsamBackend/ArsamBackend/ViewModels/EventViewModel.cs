using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ArsamBackend.Models;
using ArsamBackend.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace ArsamBackend.ViewModels
{

    public class InputEventViewModel
    {

        [Required]
        public string Name { get; set; }

        [Required]
        public bool IsProject { get; set; }

        public string Description { get; set; }

        //public string Location { get; set; }

        [Required]
        public bool IsPrivate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public bool IsLimitedMember { get; set; }

        public int MaximumNumberOfMembers { get; set; }

        public bool BuyingTicketEnabled { get; set; }

        public List<int> Categories { get; set; }


    }

    public class OutputEventViewModel
    {
        public OutputEventViewModel(Event Event, Role? userRole = null)
        {
            Name = Event.Name;
            Description = Event.Description;
            Id = Event.Id;
            IsProject = Event.IsProject;
            Location = Event.Location;
            IsPrivate = Event.IsPrivate;
            StartDate = Event.StartDate;
            EndDate = Event.EndDate;
            IsLimitedMember = Event.IsLimitedMember;
            MaximumNumberOfMembers = Event.MaximumNumberOfMembers;
            EventMembers = Event.EventMembers.Select(x => new EventOutputAppUserViewModel(x)).ToList();
            Creator = new EventOutputAppUserViewModel(Event.Creator);
            Images = Event.Images.Select(x => new OutputEventImageViewModel(x)).ToList();
            Categories = CategoryService.ConvertCategoriesToList(Event.Categories);
            Tasks = Event.Tasks.OrderBy(x => x.Order).Select(x => new OutputTaskViewModel(x)).ToList();
            MyRole = userRole?.ToString();
            TicketTypes = Event.TicketTypes?.Select(x => new TicketTypeEventViewModel(x)).ToList();
            BuyingTicketEnabled = Event.BuyingTicketEnabled;
            AveragedRating = Math.Round(Event.AveragedRating, 1);
            RatingCount = Event.Ratings == null ? 0 : Event.Ratings.Count;
            IsBlocked = Event.IsBlocked;
        }

        public string Name { get; set; }
        public int Id { get; set; }
        public bool IsProject { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public bool IsPrivate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsLimitedMember { get; set; }

        public int MaximumNumberOfMembers { get; set; }

        public List<EventOutputAppUserViewModel> EventMembers { get; set; }

        public List<OutputEventImageViewModel> Images { get; set; }

        public EventOutputAppUserViewModel Creator { get; set; }

        public ICollection<TicketTypeEventViewModel> TicketTypes { get; set; }

        public List<int> Categories { get; set; }

        public List<OutputTaskViewModel> Tasks { get; set; }

        public string MyRole { get; set; }

        public bool BuyingTicketEnabled { get; set; }
        public double AveragedRating { get; set; }
        public long RatingCount { get; set; }
        public bool IsBlocked { get; set; }

    }

    public class AdminOutputEventViewModel : OutputEventViewModel
    {
        public AdminOutputEventViewModel(Event Event, List<AppUser> admins, Role? userRole = null) : base(Event, userRole)
        {
            EventAdmins = admins.Select(x => new EventOutputAppUserViewModel(x)).ToList();
        }
        public List<EventOutputAppUserViewModel> EventAdmins { get; set; }

    }

    public class OutputAbstractViewModel
    {
        public OutputAbstractViewModel(Event ev)
        {
            Name = ev.Name;
            Id = ev.Id;
            IsPrivate = ev.IsPrivate;
            IsProject = ev.IsProject;
            StartDate = ev.StartDate;
            EndDate = ev.EndDate;
            Creator = ev.Creator.UserName;
            Categories = CategoryService.ConvertCategoriesToList(ev.Categories);
            EventMembers = ev.EventMembers.Select(x => x.UserName).ToList();
            EventTicketTypes = ev.TicketTypes.Select(x => x.Name).ToList();
            EventTickets = ev.Tickets.Select(x => x.User.UserName).ToList();
            BuyingTicketEnabled = ev.BuyingTicketEnabled;
            AveragedRating = Math.Round(ev.AveragedRating, 1);
            RatingCount = ev.Ratings == null ? 0 : ev.Ratings.Count;
            IsBlocked = ev.IsBlocked;
        }

        public string Name { get; set; }
        public int Id { get; set; }
        public bool IsProject { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPrivate { get; set; }
        public string Creator { get; set; }
        public List<int> Categories { get; set; }
        public List<string> EventMembers { get; set; }
        public List<string> EventTicketTypes { get; set; }
        public List<string> EventTickets { get; set; }
        public double AveragedRating { get; set; }
        public bool BuyingTicketEnabled { get; set; }
        public long RatingCount { get; set; }
        public bool IsBlocked { get; set; }

    }
}