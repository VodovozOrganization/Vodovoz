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
	}
}
