using Vodovoz.Domain.Employees;

namespace Vodovoz.Factories
{
	public class EmployeeRegistrationFactory : IEmployeeRegistrationFactory
	{
		public EmployeeRegistration CreateEmployeeRegistration() =>
			new EmployeeRegistration();
	}
}
