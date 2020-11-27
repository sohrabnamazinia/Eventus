using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class UserImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("AspNetUsers")]
        public string UserId { get; set; }
        public virtual AppUser User { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
