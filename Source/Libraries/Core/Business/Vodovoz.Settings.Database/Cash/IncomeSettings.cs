using System;
using Vodovoz.Settings.Cash;

namespace Vodovoz.Settings.Database.Cash
{
	public class IncomeSettings : IIncomeSettings
	{
		private readonly ISettingsController _settingsController;

		public IncomeSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int DefaultIncomeOrganizationId => _settingsController.GetValue<int>(nameof(DefaultIncomeOrganizationId).FromPascalCaseToSnakeCase());
	}
}
