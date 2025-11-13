namespace Shared.Exceptions
{
    public readonly struct ErrorMessage
    {
        public static readonly string UnknownException = "Ha ocurrido un error inesperado!";
        public static readonly string SecurityException = "Acceso no autorizado!";
        public static readonly string ArgumentException = "Error en parámetros recibidos!";
        public static readonly string ArgumentNullException = "Error por falta de uno o mas parámetos!";
        public static readonly string ADProfileNotFoundException = "El Correo del Usuario no tiene Perfil en el Active Directory!";
        public static readonly string UriFormatException = "Formato de ruta inválido!";
        public static readonly string HttpRequestException = "Error de comunicación!";
        public static readonly string EntityValidationException = "Error de Validación en la Entidad!";
        public static readonly string AuthorizationValidationException = "Error en Validación de las credenciales!";
        public static readonly string EmptyCollections = "No se encontraron registros para mostrar o procesar!";
        public static readonly string ErrorCreating = "Se ha producido un error en la creación del registro!";
        public static readonly string ErrorUpdating = "Se ha producido un error en la actualización del registro!";
        public static readonly string ErrorDeleting = "Se ha producido un error al eliminar el registro!";
        public static readonly string RecordExist = "Ya existe un registro con estos Datos!";
        public static readonly string UserPasswordExist = "Ya existe un registro con este Usuario y/o Contraseña!";
        public static readonly string AuthorizationException = "El usuario no tiene acceso al sistema!";
        public static readonly string TokenAuthorizationException = "El usuario no ha iniciado sesión o hay errores en el token!";
        public static readonly string ActiveDirectoryCOMExceptions = "El dominio especificado no existe o no se pudo contactar!";

        public static string AddedSuccessfully(string entity, string keyValue)
        {
            return $"El registro [{keyValue}], ha sido agregado en *{entity}* correctamente!";
        }

        public static string UpdatedSuccessfully(string entity, string keyValue)
        {
            return $"El registro [{keyValue}], ha sido modificado en *{entity}* correctamente!";
        }

        public static string DeletedSuccessfully(string entity, string keyValue)
        {
            return $"El registro [{keyValue}], ha sido eliminado de la entidad *{entity}* correctamente!";
        }

    }
}
