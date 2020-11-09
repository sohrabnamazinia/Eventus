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

        public string CreatorEmail { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public ICollection<Image> Images { get; set; }

        [Required]
        public bool IsLimitedMember { get; set; }

        public int MaximumNumberOfMembers { get; set; }

        public List<string> EventMembersEmail { get; set; }

    }
}