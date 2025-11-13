using Application.DTOs;
using MediatR;
using Shared.Services.Configurations;
using Shared.Services.Contracts;

namespace Application.Features.Utilities.SendMails
{
    public class SendMailCommand : MailRequest, IRequest<Result<bool>>
    { }

    public class SendMailAsyncCommandHandler(
        ISmtpMailService _mailService) : IRequestHandler<SendMailCommand, Result<bool>>
    {

        public async Task<Result<bool>> Handle(SendMailCommand request, CancellationToken cancellationToken)
        {
            var sendMail = await _mailService.SendAsync(request, null);

            if (sendMail)
            {
                return Result<bool>.Success(true, 1, "Correo enviado correctamente!");
            }
            else
            {
                return Result<bool>.Fail("Error en el envio de correo!", "500");
            }
        }
    }
}
