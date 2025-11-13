namespace Security.Entities.DTOs
{
    public class AuthMardomApiSettings
    {
        public string? BaseUrl { get; set; }
        public string? LocationTls { get; set; }
        public string? ApiKey { get; set; }
        public string? GetAllUsers { get; set; }
        public string? GetUserById { get; set; }
        public string? CreateUser { get; set; }
        public string? UpdateUser { get; set; }
        public string? InactivateUser { get; set; }
        public string? ResetPassword { get; set; }
        public string? ChangePassword { get; set; }
        public string? GetAllRoles { get; set; }
        public string? GetRoleById { get; set; }
        public string? CreateRole { get; set; }
        public string? CreateRolesCompanyPermissions { get; set; }
        public string? UpdateRolesCompanyPermissions { get; set; }
        public string? UpdateRole { get; set; }
        public string? GetAllUserRole { get; set; }
        public string? GetUserRoleById { get; set; }
        public string? CreateUserRole { get; set; }
        public string? UpdateUserRole { get; set; }
        public string? GetAllUserLocation { get; set; }
        public string? GetUserLocationById { get; set; }
        public string? CreateUserLocation { get; set; }
        public string? UpdateUserLocation { get; set; }
        public string? GetAllPermission { get; set; }
        public string? GetRolePermissionById { get; set; }
        public string? Login { get; set; }
    }
}
