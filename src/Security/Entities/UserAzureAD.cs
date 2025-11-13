namespace Security.Entities
{
    public class UserAzureAD
    {
        public required string Usua { get; set; }
        public required string DisNam { get; set; }
        public required string Mail { get; set; }
        public string? Cod { get; set; }
    }
}
