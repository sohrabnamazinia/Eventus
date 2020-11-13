using ArsamBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.ViewModels
{
    public class FilterEventsViewModel
    {
        #nullable enable
        public string? Name { get; set ; }
        public DateTime? DateMin { get; set; }
        public DateTime? DateMax { get; set; }
        public bool? IsPrivate { get; set; }
        public int? MembersCountMin { get; set; }
        public int? MembersCountMax { get; set; }
    }
}
