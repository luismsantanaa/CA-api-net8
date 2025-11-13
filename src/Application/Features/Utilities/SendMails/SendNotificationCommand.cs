using System.Linq.Expressions;
using Application.DTOs;
using Domain.Entities.Shared;
using MediatR;
using Persistence.Repositories.Contracts;
using Shared.Extensions;
using Shared.Extensions.Contracts;
using Shared.Services.Configurations;
using Shared.Services.Contracts;
using Shared.Services.Enums;

namespace Application.Features.Utilities.SendMails
{
    public class SendMailNotificationCommand : IRequest<Result<bool>>
    {
        public string? TemplateId { get; set; }
        public List<string>? Attach { get; set; } = null;
        public MailNotificationTypes? NotificationTypeId { get; set; }
    }

    public class SendMailNotificationCommandHandler(
        IRepositoryFactory _repositoryFactory,
        ISmtpMailService _mailService) : IRequestHandler<SendMailNotificationCommand, Result<bool>>
    {

        public async Task<Result<bool>> Handle(SendMailNotificationCommand request, CancellationToken cancellationToken)
        {
            var repo = _repositoryFactory.GetRepository<MailNotificationTemplate>();

            var guid = Guid.Parse(request.TemplateId!);

            Expression<Func<MailNotificationTemplate, bool>> criterial = (x => x.Id == guid && (bool)x.Active!);

            var exits = await repo.GetFirstAsync(criterial, cancellationToken)!;
            ThrowException.Exception.IfObjectClassNull(exits, request.TemplateId!);

            var mail = new MailRequest();

            switch (request.NotificationTypeId)
            {
                case MailNotificationTypes.TEST_MAIL:
                    mail = GetTestNotification(exits, request.Attach);
                    break;
                default:
                    break;
            }

            var sendMail = await _mailService.SendAsync(mail, exits.PathImages!).ConfigureAwait(false);

            if (sendMail)
            {
                return Result<bool>.Success(true, 1, "Correo enviado correctamente!");
            }
            else
            {
                return Result<bool>.Fail("Error en el envio de correo!", "500");
            }
        }

        private static MailRequest GetTestNotification(MailNotificationTemplate notification, List<string>? attach)
        {
            var mail = new MailRequest
            {
                To = new List<string>() { "lsantana@mardom.com" },
                Subject = notification.Suject!,
                Body = notification.BodyHtml!,
                IsNotification = true,
                Attach = attach
            };

            return mail;
        }
    }
}
