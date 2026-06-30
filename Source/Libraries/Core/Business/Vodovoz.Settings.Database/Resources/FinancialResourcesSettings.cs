using System;
using Vodovoz.Settings.Resources;

namespace Vodovoz.Settings.Database.Resources
{
	public class FinancialResourcesSettings : IFinancialResourcesSettings
	{
		private readonly ISettingsController _settingsController;

		public FinancialResourcesSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string BankStatementsDirectory => _settingsController.GetStringValue("bank_statements_directory");
	}
}
