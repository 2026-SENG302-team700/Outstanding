using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SENG302.Api.Services;

/**
 * All the different email templates
 */
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
    private readonly EmailSettings _settings;
    private readonly ISmtpClientWrapper _smtpClient;
    
    public EmailService(IOptions<EmailSettings> options, ISmtpClientWrapper smtpClient)
    {
        _settings = options.Value;
        _smtpClient = smtpClient;
    }
    
    /// <summary>
    /// This method is the main method for sending emails to users. I takes a html template, an email address and a dictionary that includes
    /// key information to about the email being sent.
    /// </summary>
    /// <param name="toEmail">The address to send the email to</param>
    /// <param name="template">The html template of the email to send</param>
    /// <param name="model">The values to inject into the email template</param>
    /// <returns></returns>
    public async Task SendEmailAsync(string toEmail, EmailTemplate template, Dictionary<string, string> model)
    {
        var (subject, htmlBody) = await RenderAsync(template, model);
        
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Outstanding", _settings.FromEmail));
        message.To.Add(new MailboxAddress(model["DISPLAY_NAME"], toEmail));
        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = StripHtml(htmlBody)
        }.ToMessageBody();

        
        if (OperatingSystem.IsWindows())
        {
            using var client = new SmtpClient();
            client.CheckCertificateRevocation = false; 
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.FromEmail, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return;
        }
        
        await _smtpClient.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await _smtpClient.AuthenticateAsync(_settings.FromEmail, _settings.Password);
        await _smtpClient.SendAsync(message);
        await _smtpClient.DisconnectAsync(true);
    }

    /// <summary>
    /// Find the correct template file and then call the method to split the html file into the subject and the html body,
    /// then inject the values into the body and subject.
    /// </summary>
    /// <param name="template">The template that is being sent</param>
    /// <param name="model">The Dictionary containing the user information</param>
    /// <returns></returns>
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

    /// <summary>
    /// Parse the raw html and split it into the subject and the body
    /// </summary>
    /// <param name="raw">The raw text in the html file</param>
    /// <returns>The split up subject and body</returns>
    /// <exception cref="InvalidOperationException"></exception>
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
/// <summary>
/// The class that secrets are injected into.
/// </summary>
public class EmailSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string FromEmail { get; set; } = "";
    public string Password { get; set; } = "";
}