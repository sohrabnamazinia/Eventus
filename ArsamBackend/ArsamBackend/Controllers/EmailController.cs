using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize]
    public class EmailController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> SendEmail()
        {
            MimeMessage message = new MimeMessage();
            MailboxAddress sender = new MailboxAddress("admin", "sohrabsn7@gmail.com");
            MailboxAddress reciever = new MailboxAddress("User", "sohrabsn7@gmail.com");
            message.From.Add(sender);
            message.To.Add(reciever);
            message.Subject = "this is email subject";
            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = "<h1>Hello world<h1>";
            bodyBuilder.TextBody = "hello!";
            message.Body = bodyBuilder.ToMessageBody();
            SmtpClient client = new SmtpClient();
            client.Connect("smtp.45.82.136.84", 5000, false);
            client.Authenticate("sohrabsn7", "god.sn7.cr7");
            client.Send(message);
            client.Disconnect(true);
            client.Dispose();
            //client.Authenticate("sohrab@gmail.com", "");
            return Ok();
        }
        


        

    }
}
