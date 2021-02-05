using ArsamBackend.Controllers;
using ArsamBackend.Models;
using ArsamBackend.Services;
using ArsamBackend.Utilities;
using ArsamBackend.ViewModels;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class EmailService : IEmailService
    {
        private readonly IWebHostEnvironment env;
        private readonly AppDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ILogger<AccountController> logger;
        private readonly IDataProtectionProvider dataProtectionProvider;
        private readonly DataProtectionPurposeStrings dataProtectionPurposeStrings;
        private readonly IJWTService jWTHandler;
        private readonly IMinIOService minIO;
        private readonly IConfiguration config;

        public EmailService(IWebHostEnvironment env, AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AccountController> logger, IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurposeStrings, IJWTService jWTHandler, IMinIOService minIO, IConfiguration config)
        {
            this.env = env;
            this.context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.dataProtectionProvider = dataProtectionProvider;
            this.dataProtectionPurposeStrings = dataProtectionPurposeStrings;
            this.jWTHandler = jWTHandler;
            this.minIO = minIO;
            this.config = config;
        }

        public void SendEmail(MimeMessage message, BodyBuilder bodyBuilder)
        {
            SmtpClient client = new SmtpClient();
            try
            {
                client.Connect(Constants.SMTPGoogleDomain, Constants.SMTPPort, false);
                message.Body = bodyBuilder.ToMessageBody();
                client.Authenticate(Constants.ProjectEmail, config.GetValue<string>("EventusEmailPassword"));
                client.Send(message);
                client.Disconnect(true);
                client.Dispose();
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
        }

        public void SendEmailConfirmation(SendEmailConfirmationViewModel model)
        {
            var pathToFile = env.WebRootPath + Constants.ConfirmAccountRegisterationViewPath;
                            
            MimeMessage message = new MimeMessage();
            MailboxAddress sender = new MailboxAddress(Constants.ProjectSender, Constants.ProjectEmail);
            MailboxAddress reciever = new MailboxAddress(Constants.ProjectReciever, model.Email);
            message.From.Add(sender);
            message.To.Add(reciever);
            message.Subject = Constants.EmailConfirmationSubject;
            BodyBuilder bodyBuilder = new BodyBuilder();
            using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
            {
                bodyBuilder.HtmlBody = SourceReader.ReadToEnd();
            }

            bodyBuilder.HtmlBody = string.Format(bodyBuilder.HtmlBody, model.FirstName, model.ConfirmationLink, model.Email, model.Username);
            SendEmail(message, bodyBuilder);
        }

        public void SendTicketNotification(SendTicketNotificationViewModel model)
        {
            var pathToFile = env.WebRootPath + Constants.BuyTicketResult;

            MimeMessage message = new MimeMessage();
            MailboxAddress sender = new MailboxAddress(Constants.ProjectSender, Constants.ProjectEmail);
            MailboxAddress reciever = new MailboxAddress(Constants.ProjectReciever, model.Email);
            message.From.Add(sender);
            message.To.Add(reciever);
            message.Subject = Constants.TicketBoughtResult;
            BodyBuilder bodyBuilder = new BodyBuilder();
            using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
            {
                bodyBuilder.HtmlBody = SourceReader.ReadToEnd();
            }

            bodyBuilder.HtmlBody = string.Format(bodyBuilder.HtmlBody, model.FirstName, model.TicketTypeName, model.EventName, model.price);
            SendEmail(message, bodyBuilder);
        }
    }
}
