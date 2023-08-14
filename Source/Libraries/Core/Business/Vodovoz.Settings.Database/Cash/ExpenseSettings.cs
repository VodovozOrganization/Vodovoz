using System;
using Vodovoz.Settings.Cash;

namespace Vodovoz.Settings.Database.Cash
{
	public class ExpenseSettings : IExpenseSettings
	{
		private readonly ISettingsController _settingsController;

		public ExpenseSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int DefaultChangeOrganizationId => _settingsController.GetValue<int>(nameof(DefaultChangeOrganizationId).FromPascalCaseToSnakeCase());

		public int DefaultExpenseOrganizationId => _settingsController.GetValue<int>(nameof(DefaultExpenseOrganizationId).FromPascalCaseToSnakeCase());
	}
}
