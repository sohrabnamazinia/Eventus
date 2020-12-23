using ArsamBackend.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.ViewModels
{
    public class RateEventViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public RatingStar Stars { get; set; }
    }
}
