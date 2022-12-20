using System;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Controllers
{
	public interface IEmployeeRegistrationVersionController
	{
		string AddNewRegistrationVersion(DateTime? startDate, EmployeeRegistration employeeRegistration);
		void ChangeVersionStartDate(EmployeeRegistrationVersion version, DateTime newStartDate);
	}
}
