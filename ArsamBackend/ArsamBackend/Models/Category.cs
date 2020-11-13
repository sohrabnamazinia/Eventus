using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    [Flags]
    public enum Category
    {
        Race = 1,
        Performance = 2,
        Conference = 4,
        Fundraiser = 8,
        Festival = 16,
        SocialEvent = 32
    }
}
