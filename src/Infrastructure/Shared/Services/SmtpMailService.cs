using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;
using Polly;
using Polly.Retry;
using Shared.Services.Configurations;
using Shared.Services.Contracts;
using System.Net.Sockets;

namespace Shared.Services
{
    public class SmtpMailService : ISmtpMailService
    {
        private readonly EMailSettings? _emailSettings;
        private readonly ILogger<SmtpMailService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly int _timeoutSeconds = 30;

        public SmtpMailService(
            IOptions<EMailSettings> mailSettings, 
            ILogger<SmtpMailService> logger)
        {
            _emailSettings = mailSettings.Value;
            _logger = logger;

            // Configure retry policy for transient errors
            _retryPolicy = Policy
                .Handle<SocketException>()
                .Or<TimeoutException>()
                .Or<IOException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, 
                            "Retry {RetryCount} after {Delay}s due to {ExceptionType}",
                            retryCount, timeSpan.TotalSeconds, exception.GetType().Name);
                    });
        }

        public async Task<bool> SendAsync(
            MailRequest request, 
            string? pathImages = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate request
                if (!ValidateRequest(request))
                {
                    return false;
                }

                _logger.LogInformation("Preparing to send email. To: {Recipients}, Subject: {Subject}",
                    string.Join(", ", request.To), request.Subject);

                // Execute with retry policy
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await SendEmailAsync(request, pathImages, cancellationToken);
                });

                _logger.LogInformation("Email sent successfully to: {Recipients}", string.Join(", ", request.To));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email after retries. To: {Recipients}, Subject: {Subject}",
                    string.Join(", ", request.To ?? new List<string>()), request.Subject);
                return false;
            }
        }

        private async Task SendEmailAsync(
            MailRequest request, 
            string? pathImages, 
            CancellationToken cancellationToken)
        {
            using var smtp = new SmtpClient
            {
                Timeout = _timeoutSeconds * 1000 // Convert to milliseconds
            };

            try
            {
                // Build email
                var email = BuildEmailMessage(request, pathImages);

                // Connect with timeout
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

                _logger.LogDebug("Connecting to SMTP server: {Host}:{Port}", _emailSettings!.Host, _emailSettings.Port);

                await smtp.ConnectAsync(
                    _emailSettings.Host, 
                    _emailSettings.Port, 
                    SecureSocketOptions.StartTlsWhenAvailable, 
                    cts.Token);

                await smtp.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password, cts.Token);

                await smtp.SendAsync(email, cts.Token);

                await smtp.DisconnectAsync(true, cts.Token);

                _logger.LogDebug("SMTP connection closed successfully");
            }
            catch (Exception)
            {
                // Ensure disconnection on error
                if (smtp.IsConnected)
                {
                    try
                    {
                        await smtp.DisconnectAsync(true, CancellationToken.None);
                    }
                    catch (Exception disconnectEx)
                    {
                        _logger.LogWarning(disconnectEx, "Error disconnecting SMTP client");
                    }
                }
                throw;
            }
        }

        private MimeMessage BuildEmailMessage(MailRequest request, string? pathImages)
        {
            var email = new MimeMessage();
            var builder = new BodyBuilder();
            var body = request.Body;

            // Sender
            email.Sender = MailboxAddress.Parse(_emailSettings!.From);

            // Recipients
            foreach (var to in request.To)
            {
                email.To.Add(MailboxAddress.Parse(to));
            }

            // CC
            if (request.Cc != null && request.Cc.Count >= 1)
            {
                foreach (var cc in request.Cc)
                {
                    email.Cc.Add(MailboxAddress.Parse(cc));
                }
            }

            email.Subject = request.Subject;

            // Handle notification images
            if (request.IsNotification && !string.IsNullOrEmpty(pathImages))
            {
                body = ProcessNotificationImages(builder, pathImages, body);
            }

            // Attachments
            if (request.Attach != null && request.Attach.Count >= 1)
            {
                foreach (var attach in request.Attach)
                {
                    if (File.Exists(attach))
                    {
                        builder.Attachments.Add(attach);
                    }
                    else
                    {
                        _logger.LogWarning("Attachment file not found: {Attachment}", attach);
                    }
                }
            }

            builder.HtmlBody = body;
            email.Body = builder.ToMessageBody();

            return email;
        }

        private string? ProcessNotificationImages(BodyBuilder builder, string pathImages, string? body)
        {
            try
            {
                var logoPath = Path.Combine(pathImages, "Logo.png");
                var announcedPath = Path.Combine(pathImages, "Anounced.png");
                var footerPath = Path.Combine(pathImages, "Footer.png");

                if (File.Exists(logoPath))
                {
                    var imageLogo = builder.LinkedResources.Add(logoPath);
                    imageLogo.ContentId = MimeUtils.GenerateMessageId();
                    body = body!.Replace("@logo", imageLogo.ContentId);
                }

                if (File.Exists(announcedPath))
                {
                    var imageAnounced = builder.LinkedResources.Add(announcedPath);
                    imageAnounced.ContentId = MimeUtils.GenerateMessageId();
                    body = body!.Replace("@anounced", imageAnounced.ContentId);
                }

                if (File.Exists(footerPath))
                {
                    var imageFooter = builder.LinkedResources.Add(footerPath);
                    imageFooter.ContentId = MimeUtils.GenerateMessageId();
                    body = body!.Replace("@footer", imageFooter.ContentId);
                }

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing notification images from path: {Path}", pathImages);
                return body;
            }
        }

        private bool ValidateRequest(MailRequest request)
        {
            if (request == null)
            {
                _logger.LogWarning("Email request is null");
                return false;
            }

            if (request.To == null || request.To.Count == 0)
            {
                _logger.LogWarning("Email request has no recipients");
                return false;
            }

            // Validate email formats
            foreach (var email in request.To)
            {
                if (!IsValidEmail(email))
                {
                    _logger.LogWarning("Invalid email format: {Email}", email);
                    return false;
                }
            }

            if (request.Cc != null)
            {
                foreach (var email in request.Cc)
                {
                    if (!IsValidEmail(email))
                    {
                        _logger.LogWarning("Invalid CC email format: {Email}", email);
                        return false;
                    }
                }
            }

            if (string.IsNullOrEmpty(request.Subject))
            {
                _logger.LogWarning("Email subject is empty");
                return false;
            }

            if (string.IsNullOrEmpty(request.Body))
            {
                _logger.LogWarning("Email body is empty");
                return false;
            }

            return true;
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
