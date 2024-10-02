using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Complaints;

namespace Vodovoz.Settings.Database.Complaints
{
	public class ComplaintSettings : IComplaintSettings
	{
		private readonly ISettingsController _settingsController;
		private const string _complaintResultOfEmployeesIsGuiltyIdParameterName = "ComplaintResultOfEmployeesIsGuiltyId";
		private const string _guiltProvenComplaintResultParameterName = "GuiltProvenComplaintResultId";

		public ComplaintSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int SubdivisionResponsibleId => _settingsController.GetIntValue("subdivision_responsible_id");

		public int EmployeeResponsibleId => _settingsController.GetIntValue("employee_responsible_id");

		public int ComplaintResultOfEmployeesIsGuiltyId => _settingsController.GetIntValue(_complaintResultOfEmployeesIsGuiltyIdParameterName);
		public int GuiltProvenComplaintResultId => _settingsController.GetIntValue(_guiltProvenComplaintResultParameterName);
		public int IncomeCallComplaintSourceId => _settingsController.GetIntValue($"Complaints.{nameof(IncomeCallComplaintSourceId)}");
	}
}
