using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArsamBackend.Models;

namespace ArsamBackend.ViewModels
{
    public class CommentInputViewModel
    {
        public string Description { get; set; }
        public int EventId { get; set; }
        public int ParentId { get; set; } 
    }

    public class CommentOutputViewModel
    {
        public CommentOutputViewModel(Comment comment)
        {
            this.Id = comment.Id;
            this.Description = comment.Description;
            this.User = new EventOutputAppUserViewModel(comment.User);
            this.EventId = comment.Event.Id;
            this.DateTime = comment.DateTime;
            Childs = comment.Childs.Count > 0 ? comment.Childs.OrderBy(x => x.DateTime).Select(x => new CommentOutputViewModel(x)).ToList() : null; 
        }

        public int Id { get; set; }
        public string Description { get; set; }
        public EventOutputAppUserViewModel User { get; set; }
        public int EventId { get; set; }
        public DateTime DateTime { get; set; }
        public List<CommentOutputViewModel> Childs { get; set; }


    }
}