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
        public DateTime DateTime { get; set; } = DateTime.Now;
    }
    public class CommentOutputViewModel
    {
        public CommentOutputViewModel(Comment comment)
        {
            this.Description = comment.Description;
            this.User = new EventOutputAppUserViewModel(comment.User);
            this.Event = new OutputAbstractViewModel(comment.Event);
            this.DateTime = comment.DateTime;
        }

        public string Description { get; set; }
        public EventOutputAppUserViewModel User { get; set; }
        public OutputAbstractViewModel Event { get; set; }
        public DateTime DateTime { get; set; }


    }
}
