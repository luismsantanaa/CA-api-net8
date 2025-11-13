using Security.Entities;

namespace Security.Repositories.Contracts
{
    public interface IEmployeeRepository
    {
        Task<VwEmployee?> GetEmployeeByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    }
}
