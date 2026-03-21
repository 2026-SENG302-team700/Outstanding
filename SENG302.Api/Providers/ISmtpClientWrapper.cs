using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SENG302.Api.Services;

/// <summary>
/// SMTP wrapper for unit testing email service
/// </summary>
public interface ISmtpClientWrapper : IDisposable
{
    Task ConnectAsync(string host, int port, SecureSocketOptions options);
    Task AuthenticateAsync(string userName, string password);
    Task SendAsync(MimeMessage message);
    Task DisconnectAsync(bool quit);
}

/// <summary>
/// Real implementation of SMTP client used in production
/// </summary>
public class SmtpClientWrapper : ISmtpClientWrapper
{
    private readonly SmtpClient _client = new();
    public Task ConnectAsync(string host, int port, SecureSocketOptions options) => _client.ConnectAsync(host, port, options);
    public Task AuthenticateAsync(string userName, string password) => _client.AuthenticateAsync(userName, password);
    public Task SendAsync(MimeMessage message) => _client.SendAsync(message);
    public Task DisconnectAsync(bool quit) => _client.DisconnectAsync(quit);
    public void Dispose() => _client.Dispose();
}