using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace KıbrısApp3.Services
{
    public class EmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtp = _config.GetSection("SmtpSettings");

            var from = smtp["From"];
            var host = smtp["Host"];
            var port = int.Parse(smtp["Port"]);
            var username = smtp["Username"];
            var password = smtp["Password"];
            var enableSsl = bool.Parse(smtp["EnableSsl"]);

            if (string.IsNullOrEmpty(from))
                throw new Exception("SMTP ayarlarında 'From' değeri eksik");

            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var message = new MailMessage(from, toEmail, subject, body);
            message.IsBodyHtml = true; // 🔥 HTML içeriği destekle
            message.BodyEncoding = Encoding.UTF8; // 🔠 Türkçe karakter sorununa karşı

            await client.SendMailAsync(message);
        }

    }
}
