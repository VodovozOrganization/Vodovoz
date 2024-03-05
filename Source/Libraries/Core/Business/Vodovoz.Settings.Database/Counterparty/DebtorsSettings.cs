using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Counterparty;

namespace Vodovoz.Settings.Database.Counterparty
{
	public class DebtorsSettings : IDebtorsSettings
	{
		private readonly ISettingsController _settingsController;

		public DebtorsSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int GetSuspendedCounterpartyId => _settingsController.GetIntValue("HideSuspendedCounterparty");
		public int GetCancellationCounterpartyId => _settingsController.GetIntValue("HideCancellationCounterparty");
	}
}
