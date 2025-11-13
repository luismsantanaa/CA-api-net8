using Newtonsoft.Json;

namespace Security.Entities.DTOs
{
    public class RegistrationRequest
    {
        public Guid IdentityId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Company { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        [JsonIgnore]
        public string? Codigo { get; set; }
        [JsonIgnore]
        public string? Office { get; set; }
    }
}
