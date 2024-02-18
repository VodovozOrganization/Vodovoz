using Vodovoz.Settings.Employee;

namespace Vodovoz.Settings.Database.Employee
{
	public class EmployeeSettings : IEmployeeSettings
	{
		private readonly ISettingsController _settingsController;

		public EmployeeSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new System.ArgumentNullException(nameof(settingsController));
		}
		public int DefaultEmployeeRegistrationVersionId => _settingsController.GetValue<int>(nameof(DefaultEmployeeRegistrationVersionId).FromPascalCaseToSnakeCase());
		public int WorkingClothesFineTemplateId => _settingsController.GetIntValue("working_clothes_fine_template_id");
		public int MaxDaysForNewbieDriver => _settingsController.GetIntValue("max_days_for_newbie_driver");
	}
}
