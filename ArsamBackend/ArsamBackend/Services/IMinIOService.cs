using ArsamBackend.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Services
{
    public interface IMinIOService
    {
        public Task<int> UpdateUserImage(IFormFile file, AppUser user);
        public Task<bool> CheckObjectExists(string bucketName, string objectName);
        public string CreateObjectName(string fileName, string id);
        public Task<string> GenerateUrl(string id, string fileName);
        public Task<string> GenerateEventsUrl(string fileName);

        public Task<int> RemoveUserImage(AppUser user);
        public Task<int> AddImageToEvent(IFormFile file, Event Event);
        public Task<int> UpdateEventImage(IFormFile file, EventImage image);
        public Task<int> DeleteEventImage(EventImage image);

    }
}
