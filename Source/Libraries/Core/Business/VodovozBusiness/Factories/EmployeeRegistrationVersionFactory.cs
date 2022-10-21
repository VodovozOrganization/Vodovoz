using System;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Factories
{
	public class EmployeeRegistrationVersionFactory : IEmployeeRegistrationVersionFactory
	{
		private readonly IEmployeeRegistrationFactory _employeeRegistrationFactory; 
		
		public EmployeeRegistrationVersionFactory(IEmployeeRegistrationFactory employeeRegistrationFactory)
		{
			_employeeRegistrationFactory = employeeRegistrationFactory ?? throw new ArgumentNullException(nameof(employeeRegistrationFactory));
		}
		
		public EmployeeRegistrationVersion CreateEmployeeRegistrationVersion(Employee employee) =>
			new EmployeeRegistrationVersion
			{
				Employee = employee,
				EmployeeRegistration = _employeeRegistrationFactory.CreateEmployeeRegistration()
			};
	}
}
