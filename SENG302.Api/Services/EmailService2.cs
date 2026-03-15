using System.Net;
using System.Net.Mail;

namespace SENG302.Api.Services;

public interface IEmailService2
{
    Task SendEmailAsync(string email, string subject, string message);
}

public class EmailService2 : IEmailService2
{
    private readonly IConfiguration _configuration;

    public EmailService2(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    private configInfo GetConfigInfo()
    {
        // "matz gswa wium sbuy"
        return new configInfo
        {
            SMTPHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com",
            SMTPPort = int.TryParse(_configuration["EmailSettings:SmtpPort"], out var port) ? port : 587,
            SenderEmail = _configuration["EmailSettings:SmtpSender"] ?? "shiv3.pandya@gmail.com",
            SenderPassword = _configuration["EmailSettings:SmtpPassword"] ?? "error"
        };
    }
    
    public Task SendEmailAsync(string email, string subject, string message)
    {
        // Parameters: Service responsible for sending email and port for it
        configInfo config = GetConfigInfo();
        Console.Write(config.SMTPHost);
        Console.Write(config.SMTPPort);
        Console.Write(config.SenderEmail);
        Console.Write(config.SenderPassword);
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

public class configInfo
{
    public required string SMTPHost { get; set; }
    public required int SMTPPort { get; set; }
    public required string SenderEmail { get; set; }
    public required string SenderPassword { get; set; }
}
