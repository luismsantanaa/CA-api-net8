using Microsoft.EntityFrameworkCore;
using Security.Entities;
using Security.Repositories.Contracts;

namespace Security.Repositories.Concrete
{
    internal class EmployeeRepository : IEmployeeRepository
    {
        private readonly RrHhContext _context;

        public EmployeeRepository(RrHhContext context)
        {
            _context = context;
        }

        public async Task<VwEmployee?> GetEmployeeByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _context.VwEmployees
                .FirstOrDefaultAsync(e => e.UserName == userName, cancellationToken);
        }
    }
}
