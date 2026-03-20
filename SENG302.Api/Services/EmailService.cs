using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SENG302.Api.Services;

public enum EmailTemplate
{
    VerifyEmailCode,
    ChangePasswordCode,
    ResetPasswordCode,
    PasswordChangedConfirmation,
    PasswordResetConfirmation,
    ResetCancelledWarning
}

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, EmailTemplate template, Dictionary<string, string> model);
}

public class EmailService : IEmailService
{
    private readonly string noReplyEmail = "noreply.outstanding@gmail.com";
    private readonly EmailSettings _settings;

    public EmailService(EmailSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// This method is a demo spike method to test and show sending emails works
    /// </summary>
    /// <param name="toEmail">The address to send the email to</param>
    /// <param name="subject">The subject of the email being sent</param>
    /// <param name="body">The body of the email being sent</param>
    /// <returns></returns>
    public async Task SendEmailAsync(string toEmail, EmailTemplate template, Dictionary<string, string> model)
    {
        var (subject, htmlBody) = await RenderAsync(template, model);
    
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sender", noReplyEmail));
        message.To.Add(new MailboxAddress("Receiver", toEmail));
        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = StripHtml(htmlBody)
        }.ToMessageBody();

        using var client = new SmtpClient();
        
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        Console.WriteLine("connected");
        await client.AuthenticateAsync(noReplyEmail, _settings.Password);
        Console.WriteLine("authenticated");
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
        Console.WriteLine("email sent");
    }

    private async Task<(string Subject, string HtmlBody)> RenderAsync(EmailTemplate template, Dictionary<string, string> model)
    {
        var fileName = template + ".html"; // e.g. VerifyEmailCode.html
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "EmailTemplates", fileName);

        var raw = await File.ReadAllTextAsync(path);
        // get subject and body
        var (subject, html) = ParseSubjectAndBody(raw);

        foreach (var kv in model)
        {
            var placeholder = "{{" + kv.Key + "}}";
            subject = subject.Replace(placeholder, kv.Value);
            html = html.Replace(placeholder, kv.Value);
        }

        return (subject, html);
    }

    private static (string Subject, string HtmlBody) ParseSubjectAndBody(string raw)
    {
        raw = raw.Replace("\r\n", "\n");

        var lines = raw.Split('\n');
        if (lines.Length == 0 || !lines[0].StartsWith("Subject:", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Email template missing 'Subject:' header on first line");
        }

        var subject = lines[0].Substring("Subject:".Length).Trim();
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new InvalidOperationException("Email template subject is empty");
        }

        // find the first blank line after the subject header
        var i = 1;
        while (i < lines.Length && lines[i].Trim().Length != 0)
        {
            i++;
        }

        // skip blank lines
        if (i < lines.Length) i++;

        var htmlBody = string.Join("\n", lines.Skip(i));
        if (string.IsNullOrWhiteSpace(htmlBody))
        {
            throw new InvalidOperationException("Email template body is empty.");
        }

        return (subject, htmlBody);
    }

    private static string StripHtml(string html) =>
        System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
}

public sealed class EmailSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;

    public string? Password { get; set; } = "";
    public string FromName { get; set; } = "Outstanding";
}
