using ArsamBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace ArsamBackend.Services
{
    public interface IJWTService
    {
        public string GenerateToken(AppUser user);
        public string GetRawJTW(string jwt);
        public string GetClaim(string token, string claimType);
        public Task<AppUser> FindUserByTokenAsync(string authorization, AppDbContext context);
        public Role? FindRoleByToken(string authorization, int eventId);
    }
}
