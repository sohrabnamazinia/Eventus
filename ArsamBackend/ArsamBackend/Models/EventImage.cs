using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class EventImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Event")] 
        public int EventId { get; set; }
        public virtual Event Event { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string ImageLink { get; set; }
        public long Size { get; set; }
    }
}

