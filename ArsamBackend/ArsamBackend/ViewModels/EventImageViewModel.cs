using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArsamBackend.Models;
using ArsamBackend.Utilities;

namespace ArsamBackend.ViewModels
{
    public class OutputEventImageViewModel
    {
        public OutputEventImageViewModel(EventImage image)
        {
            Image = Convert.ToBase64String(
                File.ReadAllBytes(Path.GetFullPath(Constants.EventImagesPath) + image.FileName));
            ImageId = image.Id;
        }

        public string Image { get; set; }

        public int ImageId { get; set; }

    }
}
