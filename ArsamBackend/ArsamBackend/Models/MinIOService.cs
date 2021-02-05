using ArsamBackend.Services;
using ArsamBackend.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class MinIOService : IMinIOService
    {
        private MinioClient minio;
        public const string EventsBucketName = "events";
        public const string UsersBucketName = "users";
        public const string UsersImagePostfix = "_Image";
        private readonly IConfiguration _config;
        private readonly AppDbContext context;

        public MinIOService(IConfiguration configuration, AppDbContext context)
        {
            this._config = configuration;
            this.context = context;
            minio = new MinioClient(Constants.ProjectIpAddressPort, _config.GetValue<string>("AccessKey"), _config.GetValue<string>("SecretKey"));
        }
        public async Task<int> UpdateUserImage(IFormFile file, AppUser user)
        {
            bool BucketFound = await minio.BucketExistsAsync(UsersBucketName);
            if (!BucketFound) await minio.MakeBucketAsync(UsersBucketName);
            if (user.ImageName != null)
            {
                await minio.RemoveObjectAsync(UsersBucketName, user.ImageName);
                user.ImageName = null;
                user.ImageLink = null;
            }
            var NewImageName = CreateObjectName(file.FileName, user.Id);
            await minio.PutObjectAsync(UsersBucketName, NewImageName, file.OpenReadStream(), file.Length);
            user.ImageName = NewImageName;
            user.ImageLink = GenerateUrl(user.Id, file.FileName).Result;
            context.SaveChanges();
            return 1;
        }

        public async Task<bool> CheckObjectExists(string bucketName, string objectName)
        {
            try
            {
                await minio.StatObjectAsync(bucketName, objectName);
            }
            catch (MinioException e)
            {
                return false;
            }
            return true;
        }

        public string CreateObjectName(string fileName, string id)
        {
            return id + "." + Constants.GetFileNameExtension(fileName);
        }

        public async Task<string> GenerateUrl(string id, string fileName)
        {
            if (id == null || fileName == null) return null;
            return await minio.PresignedGetObjectAsync(UsersBucketName, CreateObjectName(fileName, id), Constants.PresignedGetObjectExpirationPeriod);
        }

        public async Task<int> RemoveUserImage(AppUser user)
        {
            await minio.RemoveObjectAsync(UsersBucketName, user.ImageName);
            user.ImageName = null;
            user.ImageLink = null;
            context.SaveChanges();
            return 1;
        }

        public async Task<int> AddImageToEvent(IFormFile file, Event Event)
        {
            bool eventsBucketFound = await minio.BucketExistsAsync(EventsBucketName);
            if (!eventsBucketFound) await minio.MakeBucketAsync(EventsBucketName);

            var imageId = Guid.NewGuid().ToString().Replace("-", "");
            var newImageName = CreateObjectName(file.FileName, imageId);
            await minio.PutObjectAsync(EventsBucketName, newImageName, file.OpenReadStream(), file.Length);

            var image = new EventImage()
            {
                EventId = Event.Id,
                Event = Event,
                ContentType = file.ContentType,
                FileName = newImageName,
                Size = file.Length / 1000,
                ImageLink = GenerateEventsUrl(newImageName).Result
            };
            Event.Images.Add(image);
            await context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> UpdateEventImage(IFormFile file, EventImage image)
        {
            bool eventsBucketFound = await minio.BucketExistsAsync(EventsBucketName);
            if (!eventsBucketFound) await minio.MakeBucketAsync(EventsBucketName);

            await minio.RemoveObjectAsync(EventsBucketName, image.FileName);

            var imageId = Guid.NewGuid().ToString().Replace("-", "");
            var newImageName = CreateObjectName(file.FileName, imageId);
            await minio.PutObjectAsync(EventsBucketName, newImageName, file.OpenReadStream(), file.Length);

            image.FileName = newImageName;
            image.ContentType = file.ContentType;
            image.Size = file.Length / 1000;
            image.ImageLink = GenerateEventsUrl(newImageName).Result;

            await context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> DeleteEventImage(EventImage image)
        {
            await minio.RemoveObjectAsync(EventsBucketName, image.FileName);

            context.EventImages.Remove(image);
            await context.SaveChangesAsync();

            return 1;
        }

        public async Task<string> GenerateEventsUrl(string fileName)
        {
            if (fileName == null) return null;
            return await minio.PresignedGetObjectAsync(EventsBucketName,fileName, Constants.PresignedGetObjectExpirationPeriod);
        }
    }
}
