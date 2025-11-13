using Domain.Entities.Shared;

namespace Persistence.Seeds.Examples
{
    internal class MailNotificationData
    {
        public MailNotificationTemplate GetMailNotificationTemplate()
        {
            var testFile = "C:\\Users\\lsantana\\Desktop\\clean.pptx";
            var notification = new MailNotificationTemplate();
            notification.Description = "Notificaci&oacute;n de Prueba";
            notification.Suject = "MARDOM CQRS Template | Prueba de Notificación";
            notification.BodyHtml = "<!DOCTYPE html>" +
                "<body>" +
                    "<table style=\"margin-left:25px;border-collapse:collapse;width:80%;background-color:#ffffff;\" border=\"0\">" +
                        "<tbody>" +
                            "<tr>" +
                                "<td style=\"width:100%; background-color:#002060;\" colspan=\"2\"><img style=\"margin-left: 3em;\" src=\"cid:@logo\" alt=\"Logo\" height=\"60px;\"/></td>" +
                            "</tr>" +
                            "<tr>" +
                                "<td style=\"width:60%;\">" +
                                    "<p style=\"text-align:justify; margin-top: 4em;\"> Estimados Usuarios,</p>" +
                                    "<p style=\"text-align:justify;\"> Este es un correo de<strong>Prueba</strong>, favor Obviar.</p>" +
                                    "<p style=\"text-align:justify;\">Atentamente,</p>" +
                                    "<p style=\"text-align:justify;\"><strong>Dpto.Desarrollo MARDOM</strong>.</p>" +
                                "</td>" +
                                "<td style=\"width: 40%;\"><img src=\"cid:@anounced\" alt=\"Anounced\" width=\"100%\" height=\"100%\"/></td>" +
                            "</tr>" +
                            "<tr>" +
                                "<td style=\"width:100%;\" colspan=\"2\"><img src=\"cid:@footer\" alt=\"Footer\" width=\"100%\" height=\"125px\"/></td>" +
                            "</tr>" +
                        "</tbody>" +
                    "</table>" +
                "</body>";
            if (File.Exists(testFile))
            {
                notification.PathImages = testFile;
            }
            else
            {
                notification.PathImages = "Not Path Image";
            }

            return notification;
        }
    }
}
