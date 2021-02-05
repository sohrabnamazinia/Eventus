using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Description { get; set; }
        public virtual AppUser User { get; set; }
        public string UserId { get; set; }
        public virtual Event Event { get; set; }
        public int EventId { get; set; }
        public DateTime DateTime { get; set; }
        public virtual Comment Parent { get; set; }
        public int? ParentId { get; set; }
        public virtual List<Comment> Childs { get; set; }
    }
}