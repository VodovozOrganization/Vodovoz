using System;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Controllers
{
	public interface IEmployeeRegistrationController
	{
		void AddNewRegistrationVersion(DateTime? startDate, EmployeeRegistration employeeRegistration);
		void ChangeVersionStartDate(EmployeeRegistrationVersion version, DateTime newStartDate);
	}
}
