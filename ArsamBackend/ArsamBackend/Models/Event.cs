using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using ArsamBackend.ViewModels;

namespace ArsamBackend.Models
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Required]
        public bool IsProject { get; set; }//type of event ->project event & normal event

        public string Description { get; set; }

        public string Location { get; set; }

        public bool IsPrivate { get; set; }

        public virtual AppUser Creator { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public virtual ICollection<EventImage> Images { get; set; }

        [Required]
        public bool IsLimitedMember { get; set; }
        public bool BuyingTicketEnabled { get; set; }

        public int MaximumNumberOfMembers { get; set; }
        
        public virtual ICollection<Ticket> Tickets { get; set; }
        public double AveragedRating { get; set; }

        public virtual ICollection<TicketType> TicketTypes { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }

        public virtual List<AppUser> EventMembers { get; set; }

        public virtual List<Task> Tasks { get; set; }

        public virtual Category Categories { get; set; }
    }

    public enum Role
    {
        Creator,
        Admin,
        Member
    }
}