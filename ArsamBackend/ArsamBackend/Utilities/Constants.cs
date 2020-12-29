using Minio;
using System;
using System.Collections.Generic;
using System.IO;
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
        public const string OneImageRequiredError = "One image must be set!";
        public const string ImageNotFound = "Image not found!";
        public const string EventImagesPath = "Images/Events/";
        public const string UserImagesPath = "Images/Users/";
        public const string ProjectIpAddressPort = "45.82.136.84:9000";
        public const int PresignedGetObjectExpirationPeriod = 60 * 60 * 24 * 7;
        public const int MaxImageSizeByte = 5000000;
        public static readonly byte[] png = new byte[] { 137, 80, 78, 71 };
        public static readonly byte[] tiff = new byte[] { 73, 73, 42 };
        public static readonly byte[] tiff2 = new byte[] { 77, 77, 42 };
        public static readonly byte[] jpeg = new byte[] { 255, 216, 255, 224 };
        public static readonly byte[] jpeg2 = new byte[] { 255, 216, 255, 225 };
        public static readonly string jpg;
        public static readonly string jfif;
        public static readonly string tif;
        public static readonly string ConfirmAccountRegisterationViewPath = Path.DirectorySeparatorChar.ToString() + "Templates" + Path.DirectorySeparatorChar.ToString() + "EmailTemplate" + Path.DirectorySeparatorChar.ToString() + "EmailConfirmation.html";
        public static readonly string BuyTicketResult = Path.DirectorySeparatorChar.ToString() + "Templates" + Path.DirectorySeparatorChar.ToString() + "EmailTemplate" + Path.DirectorySeparatorChar.ToString() + "BuyTicketResult.html";
        public static readonly string SMTPGoogleDomain = "smtp.gmail.com";
        public static readonly int SMTPPort = 587;
        public static readonly string ProjectEmail = "eventus.arsam@gmail.com";
        public static readonly string ProjectSender = "Eventus Team";
        public static readonly string ProjectReciever = "User";
        public static readonly string EmailConfirmationSubject = "Confirm your Email";
        public static readonly string TicketBoughtResult = "You bought the ticket successfully!";
        public static readonly string EmailConfirmationCallBackUrl = "https://localhost:44373/api/account/confirmemail";
        public static readonly string EmailSuccessfullyConfirmedMessage = "Email is Successfully Confirmed!";
        public static readonly string EmailFailedToConfirmedMessage = "Email is not Confirmed!\nplease try again later!\ncontact eventus.arsam@gmail.com for more support";

        public static bool FileFormatChecker(byte[] fileBytes)
        {
            var firstBytes4 = fileBytes.Take(4).ToArray();
            var firstBytes3 = firstBytes4.Take(3).ToArray();
            if (Enumerable.SequenceEqual(firstBytes3, tiff) || Enumerable.SequenceEqual(firstBytes3, tiff2)) return true;
            else if (Enumerable.SequenceEqual(firstBytes4, png) || Enumerable.SequenceEqual(firstBytes4, jpeg) || Enumerable.SequenceEqual(firstBytes4, jpeg2)) return true;
            return false;
        }

        public static bool CheckFileNameExtension(string extension)
        {
            extension = extension.ToLower().Substring(1);
            if (extension.Equals(nameof(png)) || extension.Equals(nameof(jpeg)) || extension.Equals(nameof(jpeg2)) || extension.Equals(nameof(jpg)) || extension.Equals(nameof(tiff)) || extension.Equals(nameof(tiff2)) || extension.Equals(nameof(tiff)) || extension.Equals(nameof(jfif)) || extension.Equals(nameof(tif)))
            {
                return true;
            }
            return false;
        }

        public static string GetFileNameExtension(string fileName)
        {
            var extension = fileName.Split(".");
            return extension[extension.Length - 1];
        }
    }




}
