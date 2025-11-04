namespace Security.Entities.DTOs
{
    internal class RHEmployeesResult<T> where T : class
    {
        public int Count { get; set; }
        public List<T>? Data { get; set; }
    }
}
