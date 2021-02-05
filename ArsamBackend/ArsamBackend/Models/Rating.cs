using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class Rating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public virtual AppUser User { get; set; }
        public string  UserId { get; set; }
        public virtual Event Event { get; set; }
        public int EventId { get; set; }
        public RatingStar Stars  { get; set; }
    }

    public enum RatingStar
    {
        ONE = 1,
        TWO = 2,
        THREE = 3,
        FOUR = 4,
        FIVE = 5
    }
}
