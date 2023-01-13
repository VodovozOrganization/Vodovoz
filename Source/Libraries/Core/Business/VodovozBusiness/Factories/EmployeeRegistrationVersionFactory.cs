using Vodovoz.Domain.Employees;

namespace Vodovoz.Factories
{
	public class EmployeeRegistrationVersionFactory : IEmployeeRegistrationVersionFactory
	{
		public EmployeeRegistrationVersion CreateEmployeeRegistrationVersion(Employee employee, EmployeeRegistration employeeRegistration) =>
			new EmployeeRegistrationVersion
			{
				Employee = employee,
				EmployeeRegistration = employeeRegistration
			};
	}
}
