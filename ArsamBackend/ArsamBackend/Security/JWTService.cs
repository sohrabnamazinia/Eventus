using ArsamBackend.Models;
using ArsamBackend.Services;
using ArsamBackend.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ArsamBackend.Migrations;
using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;

namespace ArsamBackend.Security
{
    public class JWTService : IJWTService
    {

        private readonly IConfiguration _config;

        public JWTService(IConfiguration config)
        {
            this._config = config;
        }

        public string GenerateToken(AppUser user)
        {
            var TokenSignKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("TokenSignKey")));
            var Creds = new SigningCredentials(TokenSignKey, SecurityAlgorithms.HmacSha512Signature);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.UserName),
                new Claim( "UserId", user.Id)
            };


            var TokenHandler = new JwtSecurityTokenHandler();
            var TokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddYears(1),
                SigningCredentials = Creds
            };

            var Token = TokenHandler.CreateToken(TokenDescriptor);

            return TokenHandler.WriteToken(Token);
        }

        public static string FindEmailByToken(string authorization)
        {
            string token = string.Empty;
            if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                token = headerValue.Parameter;
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            var userEmail = securityToken.Claims.First(claim => claim.Type == "nameid").Value;
            return userEmail;
        }
        public async Task<AppUser> FindUserByTokenAsync(string authorization, AppDbContext context)
        {
            var userEmail = FindEmailByToken(authorization);
            return await context.Users.SingleOrDefaultAsync(x => x.Email == userEmail);
        }
        public async Task<Role?> FindRoleByTokenAsync(string authorization, int eventId, AppDbContext context)
        {
            string token = string.Empty;
            if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                token = headerValue.Parameter;
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            var userId = securityToken.Claims.First(claim => claim.Type == "UserId").Value;
            if (userId == null)
                return null;
            var userRole = await context.EventUserRole.FindAsync(userId, eventId);
            return userRole?.Role;
        }

        #region utilities

        public string GetRawJTW(string jwt)
        {
            var token = string.Empty;
            if (AuthenticationHeaderValue.TryParse(jwt, out var headerValue))
            {
                token = headerValue.Parameter;
            }
            return token;
        }
        public bool ValidateToken(string token)
        {
            var TokenSignKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("TokenSignKey")));
            var TokenHandler = new JwtSecurityTokenHandler();
            try
            {
                TokenHandler.ValidateToken(token, new TokenValidationParameters 
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = TokenSignKey,
                    ValidateAudience = false,
                    ValidateIssuer = false
                }, out SecurityToken validatedToken);
            }
            catch 
            {
                return false;
            }
            return true;
        }

        public string GetClaim(string token, string claimType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            var stringClaimValue = securityToken.Claims.First(claim => claim.Type == claimType).Value;
            return stringClaimValue;
        }
        #endregion utilities

    }

}
