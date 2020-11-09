using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Utilities
{
    public static class Constants
    {
        public const string RouteName = "RoutePattern";
        public const string RoutePattern = "api/{controller}/{action}/{id?}";
        public const string ConnectionStringKey = "AppDbConnection";
        public const string MyAllowSpecificOrigins = "https://localhost:3000";
        public const string CORSPolicyName = "CORSPolicy";
        public const string PasswordAllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        public const string EmailConfirmationError = "Email has not been confirmed yet!";
        public const string InvalidLoginError = "Invalid login attempt!";
        public const string NotFoundError = "User not found!";
        public const string EventImagesPath = ("Images/Events/");
    }
}
