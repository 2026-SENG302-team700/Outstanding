using System.Net;
using System.Net.Mail;

namespace SENG302.Api.Services;

public class configInfo
{
    public string SMTPHost { get; set; }
    public int SMTPPort { get; set; }
    public string SenderEmail { get; set; }
    public string SenderPassword { get; set; }
}

public interface IEmailService2
{
    Task SendEmailAsync(string email, string subject, string message);
}

public class EmailService2 : IEmailService2
{
    public configInfo getConfigInfo()
    {
        return new configInfo
        {
            SMTPHost = Environment.GetEnvironmentVariable("SMTP_SENDER"),
            SMTPPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")),
            SenderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL"),
            SenderPassword = Environment.GetEnvironmentVariable("SENDER_PASSWORD"),
        };
    }
    
    public Task SendEmailAsync(string email, string subject, string message)
    {
        // Parameters: Service responsible for sending email and port for it
        configInfo config = getConfigInfo();
        var client = new SmtpClient(config.SMTPHost, config.SMTPPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(config.SenderEmail, config.SenderPassword)
        };

        return client.SendMailAsync(
            new MailMessage(from: config.SenderEmail,
                to: email,
                subject,
                message
            ));
    }
}