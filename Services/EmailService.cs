using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;

namespace otelrezervation.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var emailSettings = _config.GetSection("EmailSettings");
                string fromEmail = emailSettings["SenderEmail"];
                string fromName = emailSettings["SenderName"];
                string username = emailSettings["Username"];
                string password = emailSettings["Password"];
                string host = emailSettings["SmtpServer"];     
                int port = int.Parse(emailSettings["Port"]); 

                if(string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(password))
                {
                    // Eger appsettings icinde tanimli degilse (henuz mail verilmediyse) console'a bas ve cik
                    Console.WriteLine("---------------------------------------------");
                    Console.WriteLine("SMTP bilgileri eksik. Mail gonderilemedi!");
                    Console.WriteLine($"Kime: {to}");
                    Console.WriteLine($"Konu: {subject}");
                    Console.WriteLine($"Mesaj: {body}");
                    Console.WriteLine("---------------------------------------------");
                    return;
                }

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(fromName, fromEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = body };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient(); 
                // SMTP yani simple mail transfer protocol, web sitesi icin HTTP ne ise e-posta icin smtp odur. 
                // TLS yani transport layer security, verilerin sifrelenerek iletilmesini saglar. https ile ayni mantikta calisir.
                // Google SMTP için StartTls kullanıyoruz
                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls); // connect to server 
                await smtp.AuthenticateAsync(username, password); 
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Mail gonderim hatalarini yakala
                Console.WriteLine($"Mail gönderilirken bir hata oluştu: {ex.Message}");
            }
        }
    }
}
