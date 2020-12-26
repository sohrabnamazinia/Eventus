using ArsamBackend.ViewModels;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Services
{
    public interface IEmailService
    {
        public void SendEmail(MimeMessage message, BodyBuilder bodyBuilder);
        public void SendEmailConfirmation(SendEmailConfirmationViewModel model);
        public void SendTicketNotification(SendTicketNotificationViewModel model);
    }
}
