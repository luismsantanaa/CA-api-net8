namespace Application.Contracts
{
    public interface IResult<T>
    {
        public bool Succeeded { get; set; }
        public string? FriendlyMessage { get; set; }
        public T? Items { get; set; }
        public int Total { get; set; }
        public string? StatusCode { get; set; }
        public string? StackTrace { get; set; }
    }
}
