using ArsamBackend.Services;
using ArsamBackend.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class MinIOService : IMinIOService
    {
        private MinioClient minio;
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
    }
}
