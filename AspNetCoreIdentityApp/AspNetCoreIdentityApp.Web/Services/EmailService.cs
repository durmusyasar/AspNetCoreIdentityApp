using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using AspNetCoreIdentityApp.Web.OptionsModels;

namespace AspNetCoreIdentityApp.Web.Services
{
    public class EmailService : IEmailService
    {

        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _emailSettings = options.Value;
        }

        public async Task SendResetPasswordEmail(string resetPasswordEmailLink, string ToEmail)
        {
            var smptClient = new SmtpClient
            {
                Host = _emailSettings.Host,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Port = 587,
                Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.Email),
                Subject = "Localhost | Şifre sıfırlama linki",
                Body = @$"
                  <h4>Şifrenizi yenilemek için aşağıdaki linkte tıklayınız.</h4>
                  <p><a href='{resetPasswordEmailLink}'>şifre yenileme link</a></p>",
                IsBodyHtml = true
            };
            mailMessage.To.Add(ToEmail);

            await smptClient.SendMailAsync(mailMessage);



        }
    }
}
