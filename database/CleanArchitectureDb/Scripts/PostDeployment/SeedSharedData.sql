/*
    Script de carga de datos maestros compartidos
    Este script se ejecuta en cada deployment y usa MERGE para evitar duplicados
*/

PRINT 'Cargando datos maestros compartidos...'

-- MailNotificationTemplate de prueba
IF NOT EXISTS (SELECT 1 FROM [Shared].[MailNotificationTemplate] WHERE [Description] = 'Notificación de Prueba')
BEGIN
    INSERT INTO [Shared].[MailNotificationTemplate] 
    (
        [Id],
        [Active],
        [Description],
        [Suject],
        [BodyHtml],
        [PathImages],
        [CreatedBy],
        [CreatedOn],
        [Version]
    )
    VALUES 
    (
        NEWID(),
        1,
        N'Notificación de Prueba',
        N'MARDOM CQRS Template | Prueba de Notificación',
        N'<!DOCTYPE html><body><table style="margin-left:25px;border-collapse:collapse;width:80%;background-color:#ffffff;" border="0"><tbody><tr><td style="width:100%; background-color:#002060;" colspan="2"><img style="margin-left: 3em;" src="cid:@logo" alt="Logo" height="60px;"/></td></tr><tr><td style="width:60%;"><p style="text-align:justify; margin-top: 4em;"> Estimados Usuarios,</p><p style="text-align:justify;"> Este es un correo de<strong>Prueba</strong>, favor Obviar.</p><p style="text-align:justify;">Atentamente,</p><p style="text-align:justify;"><strong>Dpto.Desarrollo MARDOM</strong>.</p></td><td style="width: 40%;"><img src="cid:@anounced" alt="Anounced" width="100%" height="100%"/></td></tr><tr><td style="width:100%;" colspan="2"><img src="cid:@footer" alt="Footer" width="100%" height="125px"/></td></tr></tbody></table></body>',
        N'Not Path Image',
        NULL,
        GETUTCDATE(),
        1
    )
    
    PRINT 'Template de notificación insertado correctamente'
END
ELSE
BEGIN
    PRINT 'Template de notificación ya existe, omitiendo inserción'
END

PRINT 'Datos maestros compartidos cargados exitosamente'
GO

