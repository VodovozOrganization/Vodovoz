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
		public int DefaultEmployeeRegistrationVersionId =>
			_settingsController.GetValue<int>(nameof(DefaultEmployeeRegistrationVersionId).FromPascalCaseToSnakeCase());
		public int WorkingClothesFineTemplateId =>
			_settingsController.GetIntValue("working_clothes_fine_template_id");
		public int MaxDaysForNewbieDriver =>
			_settingsController.GetIntValue("max_days_for_newbie_driver");
		public int DefaultEmployeeForCallTask =>
			_settingsController.GetIntValue("сотрудник_по_умолчанию_для_crm");
		public int DefaultEmployeeForDepositReturnTask =>
			_settingsController.GetIntValue("сотрудник_по_умолчанию_для_задач_по_залогам");
		public int MobileAppEmployee => _settingsController.GetIntValue(nameof(MobileAppEmployee));
		public int VodovozWebSiteEmployee => _settingsController.GetIntValue(nameof(VodovozWebSiteEmployee));
		public int KulerSaleWebSiteEmployee => _settingsController.GetIntValue(nameof(KulerSaleWebSiteEmployee));
	}
}
