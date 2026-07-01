using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MobileShop.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendOrderConfirmationAsync(string to, string orderNumber, decimal total);
        Task SendOrderStatusUpdateAsync(string to, string orderNumber, string status);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];

                using var client = new System.Net.Mail.SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword)
                };

                var message = new System.Net.Mail.MailMessage(senderEmail, to, subject, body)
                {
                    IsBodyHtml = isHtml
                };

                await client.SendMailAsync(message);
               
            }
            catch (Exception ex)
            {
                
            }
        }

        public async Task SendOrderConfirmationAsync(string to, string orderNumber, decimal total)
        {
            var subject = $"Order Confirmation - {orderNumber}";
            var body = $@"
                <h2>Thank you for your order!</h2>
                <p>Your order <strong>{orderNumber}</strong> has been placed successfully.</p>
                <p>Total Amount: <strong>PKR{total:N2}</strong></p>
                <p>We will notify you once your order is shipped.</p>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendOrderStatusUpdateAsync(string to, string orderNumber, string status)
        {
            var subject = $"Order Status Update - {orderNumber}";
            var body = $@"
                <h2>Order Status Update</h2>
                <p>Your order <strong>{orderNumber}</strong> status has been updated to: <strong>{status}</strong></p>
                <p>You can track your order on our website.</p>";

            await SendEmailAsync(to, subject, body);
        }
    }
}