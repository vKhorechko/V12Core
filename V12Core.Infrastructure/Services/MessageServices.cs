using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using V12Core.Application.Interfaces;
using V12Core.Domain.Entities;

namespace V12Core.Infrastructure.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link https://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        public AuthMessageSender(IOptions<SMSoptions> smsOptionsAccessor, IOptions<EmailOptions> emailOptionsAccessor)
        {
            SMSOptions = smsOptionsAccessor.Value;
            EmailOptions = emailOptionsAccessor.Value;
        }

        public SMSoptions SMSOptions { get; }  // set only via Secret Manager
        public EmailOptions EmailOptions { get; }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация сайта", EmailOptions.Login));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(EmailOptions.SMTPserver, EmailOptions.Port, EmailOptions.UseSSL);
                await client.AuthenticateAsync(EmailOptions.Login, EmailOptions.Password);
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }

        public async Task SendSmsAsync(string number, string message)
        {
            var accountSid = SMSOptions.SMSAccountIdentification;
            var authToken = SMSOptions.SMSAccountPassword;

            TwilioClient.Init(accountSid, authToken);

            await MessageResource.CreateAsync(
              to: new PhoneNumber(number),
              from: new PhoneNumber(SMSOptions.SMSAccountFrom),
              body: message);
        }
    }
}
