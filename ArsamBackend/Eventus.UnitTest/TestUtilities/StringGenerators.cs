using ArsamBackend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eventus.UnitTest.TestUtilities
{
    public static class StringGenerators
    {
        public static string CreateRandomString()
        {
            int length = new Random().Next(20);
            var random = new Random();
            var randomString = new string(Enumerable.Repeat(Constants.PasswordAllowedUserNameCharacters, length)
                                                    .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }
    }
}
