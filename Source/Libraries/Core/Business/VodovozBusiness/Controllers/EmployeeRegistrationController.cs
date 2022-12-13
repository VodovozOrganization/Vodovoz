using System;
using System.Linq;
using MoreLinq;
using Vodovoz.Domain.Employees;
using Vodovoz.Factories;

namespace Vodovoz.Controllers
{
	public class EmployeeRegistrationController : IEmployeeRegistrationController
	{
		private readonly Employee _employee;
		private readonly IEmployeeRegistrationVersionFactory _employeeRegistrationVersionFactory;

		public EmployeeRegistrationController(
			Employee employee,
			IEmployeeRegistrationVersionFactory employeeRegistrationVersionFactory)
		{
			_employee = employee ?? throw new ArgumentNullException(nameof(employee));
			_employeeRegistrationVersionFactory =
				employeeRegistrationVersionFactory ?? throw new ArgumentNullException(nameof(employeeRegistrationVersionFactory));
		}

		public void AddNewRegistrationVersion(DateTime? startDate, EmployeeRegistration employeeRegistration)
		{
			if(!startDate.HasValue)
			{
				startDate = DateTime.Today;
			}

			var newVersion = _employeeRegistrationVersionFactory.CreateEmployeeRegistrationVersion(_employee, employeeRegistration);
			
			if(_employee.EmployeeRegistrationVersions.Any())
			{
				var lastVersion = _employee.EmployeeRegistrationVersions.MaxBy(x => x.StartDate);

				if(lastVersion != null)
				{
					if(startDate < lastVersion.StartDate.AddDays(1))
					{
						throw new ArgumentException(
							"Дата начала действия новой версии должна быть минимум на день позже, чем дата начала действия предыдущей версии",
							nameof(startDate));
					}
					lastVersion.EndDate = startDate.Value.AddMilliseconds(-1);
				}
			}

			newVersion.StartDate = startDate.Value;
			_employee.ObservableEmployeeRegistrationVersions.Insert(0, newVersion);
		}
		
		public void ChangeVersionStartDate(EmployeeRegistrationVersion version, DateTime newStartDate)
		{
			if(version == null)
			{
				throw new ArgumentNullException(nameof(version));
			}

			var previousVersion = GetPreviousVersion(version);
			if(previousVersion != null)
			{
				var newEndDate = newStartDate.AddMilliseconds(-1);
				previousVersion.EndDate = newEndDate;
			}
			version.StartDate = newStartDate;
		}
		
		private EmployeeRegistrationVersion GetPreviousVersion(EmployeeRegistrationVersion selectedVersion)
		{
			return _employee.EmployeeRegistrationVersions
				.Where(x => x.StartDate < selectedVersion.StartDate)
				.OrderByDescending(x => x.StartDate)
				.FirstOrDefault();
		}
	}
}
