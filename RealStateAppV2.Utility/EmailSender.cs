using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingLibraryWeb.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //var emailToSend = new MimeMessage();
            //emailToSend.From.Add(MailboxAddress.Parse("KingsLibrary@dotnetmastery.com"));
            //emailToSend.To.Add(MailboxAddress.Parse(email));
            //emailToSend.Subject = subject;
            //emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html){Text=htmlMessage };

            ////send email
            //using (var emailClient = new SmtpClient())// make sure tha de using statement is MailKit.Net.Smtp;
            //{
            //    emailClient.Connect("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            //    emailClient.Authenticate("20210580@itla.edu.do", "21deoctubre");
            //    emailClient.Send(emailToSend);
            //    emailClient.Disconnect(true);
            //}
            return Task.CompletedTask;  
        }   
    }
}
