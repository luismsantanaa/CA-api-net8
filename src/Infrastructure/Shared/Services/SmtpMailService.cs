using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;
using Shared.Services.Configurations;
using Shared.Services.Contracts;

namespace Shared.Services
{
    public class SmtpMailService : ISmtpMailService
    {
        private readonly EMailSettings? _emailSettings;
        private readonly ILogger<SmtpMailService> _logger;

        public SmtpMailService(IOptions<EMailSettings> mailSettings, ILogger<SmtpMailService> logger)
        {
            _emailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(MailRequest request, string? pathImages = null)
        {
            try
            {
                var email = new MimeMessage();
                var builder = new BodyBuilder();
                var body = request.Body;

                email.Sender = MailboxAddress.Parse(_emailSettings!.From);

                foreach (var to in request.To)
                {
                    email.To.Add(MailboxAddress.Parse(to));
                }

                if (request.Cc != null && request.Cc.Count >= 1)
                {
                    foreach (var cc in request.Cc)
                    {
                        email.Cc.Add(MailboxAddress.Parse(cc));
                    }
                }

                email.Subject = request.Subject;

                ///

                #region Set Image

                if (request.IsNotification && pathImages != null)
                {
                    var imageLogo = builder.LinkedResources.Add(pathImages + "\\Logo.png");
                    var imageAnounced = builder.LinkedResources.Add(pathImages + "\\Anounced.png");
                    var imageFooter = builder.LinkedResources.Add(pathImages + "\\Footer.png");
                    imageLogo.ContentId = MimeUtils.GenerateMessageId();
                    imageAnounced.ContentId = MimeUtils.GenerateMessageId();
                    imageFooter.ContentId = MimeUtils.GenerateMessageId();
                    //
                    var body1 = body!.Replace("@logo", imageLogo.ContentId);
                    var body2 = body1.Replace("@anounced", imageAnounced.ContentId);
                    body = body2.Replace("@footer", imageFooter.ContentId);
                }

                #endregion Set Image

                //

                if (request.Attach != null && request.Attach.Count >= 1)
                {
                    foreach (var attach in request.Attach)
                    {
                        builder.Attachments.Add(attach);
                    }
                }

                builder.HtmlBody = body;

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();

                smtp.Connect(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTlsWhenAvailable);

                smtp.Authenticate(_emailSettings.UserName, _emailSettings.Password);

                await smtp.SendAsync(email);

                _logger.LogInformation($"Mail Sent to:{request.To}");

                smtp.Disconnect(true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }
    }
}
