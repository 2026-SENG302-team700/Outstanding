using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SENG302.Services;

/// <summary>
/// Config class that will hold the SMTP infromation
/// </summary>
class Config
{
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPass { get; set; }
}

public interface IEmailService
{
     Task SendEmailAsync(string toEmail, string subject, string body);
}

public class EmailService : IEmailService
{
    /// <summary>
    /// This method retrieves key 'secret' information from environment variables and returns a Config object
    /// </summary>
    /// <returns>Config object containing the SMTP information</returns>
    static Config GetConfigFromEnv()
    {
        return new Config
        {
            // Get the SMTP variables from the env vars
            SmtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "localhost",
            SmtpPort = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) ? port : 25,
            SmtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? "default-user@fake.com",
            SmtpPass = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "password"
        };
    }

    /// <summary>
    /// This method is a demo spike method to test and show sending emails works
    /// </summary>
    /// <param name="toEmail">The address to send the email to</param>
    /// <param name="subject">The subject of the email being sent</param>
    /// <param name="body">The body of the email being sent</param>
    /// <returns></returns>
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        Config EmailConfig = GetConfigFromEnv();

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Sender", EmailConfig.SmtpUser));
        message.To.Add(new MailboxAddress("Receiver", toEmail));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        Console.WriteLine($"{EmailConfig.SmtpHost}");

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(EmailConfig.SmtpHost, EmailConfig.SmtpPort, SecureSocketOptions.StartTls);
            Console.WriteLine("connected");
            await client.AuthenticateAsync(EmailConfig.SmtpUser, EmailConfig.SmtpPass);
            Console.WriteLine("authenticated");
            await client.SendAsync(message);
            
            await client.DisconnectAsync(true);
        }

        Console.WriteLine("Email sent!");

    }
}
