using Security.Entities;
using Security.Repositories.Contracts;

namespace Security.Services.Concrete
{
    internal class EmployeeService
    {
        private readonly string _userName;
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(string userName, IEmployeeRepository employeeRepository)
        {
            _userName = userName;
            _employeeRepository = employeeRepository;
        }

        public async Task<VwEmployee>? GetEmployee()
        {
            return await _employeeRepository.GetEmployeeByUserNameAsync(_userName);
        }


    }
}
