using Vodovoz.Domain.Employees;

namespace Vodovoz.Factories
{
	public interface IEmployeeRegistrationVersionFactory
	{
		EmployeeRegistrationVersion CreateEmployeeRegistrationVersion(Employee employee, EmployeeRegistration employeeRegistration);
	}
}
