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
    }
}
