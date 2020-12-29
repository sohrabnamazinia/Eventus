using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using ArsamBackend.Models;
using ArsamBackend.Services;
using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Task = System.Threading.Tasks.Task;

namespace ArsamBackend.Security
{
    public class CheckIsUserPremiumMiddleWare
    {
        private readonly RequestDelegate _next;

        public CheckIsUserPremiumMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, AppDbContext db)
        {
            var token = httpContext.Request.Headers[HeaderNames.Authorization];
            if (!token.IsNullOrEmpty())
            {
                var requestedUserEmail = JWTService.FindEmailByToken(token);
                AppUser requestedUser = await db.Users.Include(x => x.InEvents)
                    .SingleOrDefaultAsync(x => x.Email == requestedUserEmail);
                if (requestedUser != null)
                {
                    if (requestedUser.ExpireDateOfPremium != null)
                    {
                        if (requestedUser.ExpireDateOfPremium < DateTime.Now)
                        {
                            var Events = requestedUser.CreatedEvents.ToList();
                            for (int i = 0; i < Events.Count - 5; i++)
                            {
                                Events[i].IsBlocked = true;
                            }

                            requestedUser.ExpireDateOfPremium = null;
                            await db.SaveChangesAsync();
                        }
                    }
                }

            }

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong: {ex.Message}");
            }
        }
    }
    public static class GlobalCustomMiddleware
    {
        public static void UseCheckUserIsPremium(this IApplicationBuilder app)
        {
            app.UseMiddleware<CheckIsUserPremiumMiddleWare>();
        }
    }
}
