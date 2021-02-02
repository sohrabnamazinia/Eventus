using ArsamBackend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ArsamBackend.Security
{
    public class TokenManagerMiddleware : IMiddleware
    {
        private readonly IJWTService jWTService;

        public TokenManagerMiddleware(IJWTService jWTService)
        {
            this.jWTService = jWTService;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var token = context.Request.Headers[HeaderNames.Authorization];
            if (token.ToString() == null)
            {
                await next(context);
                return;
            }

            var email = JWTService.FindEmailByToken(token);
            token = jWTService.GetRawJTW(token);
            var isBlocked = jWTService.IsTokenBlocked(email, token);
            if (!isBlocked)
            {
                await next(context);
                return;
            }
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
    }
}
