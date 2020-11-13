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
        public static Category BitWiseOr(List<int> categories)
        {
            Category result = 0;
            foreach (var i in categories)
                result |= (Category)i;

            return result;
        }

        [Required]
        public string Name { get; set; }
        
        [Required]
        public bool IsProject { get; set; }//type of event ->project event & normal event

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

        public List<int> Categories { get; set; }

       
    }

    public class OutputEventViewModel
    {
        public OutputEventViewModel(Event createdEvent)
        {
            Name = createdEvent.Name;
            Description = createdEvent.Description;
            Id = createdEvent.Id;
            IsProject = createdEvent.IsProject;
            Location = createdEvent.Location;
            IsPrivate = createdEvent.IsPrivate;
            StartDate = createdEvent.StartDate;
            EndDate = createdEvent.EndDate;
            IsLimitedMember = createdEvent.IsLimitedMember;
            MaximumNumberOfMembers = createdEvent.MaximumNumberOfMembers;
            EventMembers = createdEvent.EventMembers;
            Creator = new OutputAppUserViewModel(createdEvent.Creator);
            ImagesPath = createdEvent.Images.Select(x => Path.GetFullPath(Constants.EventImagesPath) + x.FileName).ToList();
            Categories = ConvertCategoriesToList(createdEvent.Categories);
        }
        private List<int> ConvertCategoriesToList(Category category)
        {
            List<int> result = new List<int>();
            foreach (Category cat in Enum.GetValues(typeof(Category)))
                if ((category & cat) != 0)
                    result.Add((int)cat);

            return result;
        }
        public string Name { get; set; }
        public int Id { get; set; }
        
        public bool IsProject { get; set; }//type of event ->project event & normal event

        public string Description { get; set; }

        public string Location { get; set; }

        public bool IsPrivate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsLimitedMember { get; set; }

        public int MaximumNumberOfMembers { get; set; }

        public List<AppUser> EventMembers { get; set; }

        public List<string> ImagesPath { get; set; }

        public OutputAppUserViewModel Creator { get; set; }

        public List<int> Categories { get; set; }

    }

    public class Output2EventViewModel
    {
        public Output2EventViewModel(Event createdEvent)
        {
            Name = createdEvent.Name;
            Description = createdEvent.Description;
            Id = createdEvent.Id;
            Location = createdEvent.Location;
            StartDate = createdEvent.StartDate;
            EndDate = createdEvent.EndDate;
            IsLimitedMember = createdEvent.IsLimitedMember;
            MaximumNumberOfMembers = createdEvent.MaximumNumberOfMembers;
            EventMembers = createdEvent.EventMembers;
            Creator = new OutputAppUserViewModel(createdEvent.Creator);
            ImagesPath = createdEvent.Images.Select(x => Path.GetFullPath(Constants.EventImagesPath) + x.FileName).ToList();
            Categories = ConvertCategoriesToList(createdEvent.Categories);
        }
        private List<int> ConvertCategoriesToList(Category category)
        {
            List<int> result = new List<int>();
            foreach (Category cat in Enum.GetValues(typeof(Category)))
                if ((category & cat) != 0)
                    result.Add((int)cat);

            return result;
        }
        public string Name { get; set; }

        public int Id { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsLimitedMember { get; set; }

        public int MaximumNumberOfMembers { get; set; }

        public List<AppUser> EventMembers { get; set; }

        public FileStream Image { get; set; }

        public OutputAppUserViewModel Creator { get; set; }

        public List<string> ImagesPath { get; set; }

        public List<int> Categories { get; set; }

    }
}