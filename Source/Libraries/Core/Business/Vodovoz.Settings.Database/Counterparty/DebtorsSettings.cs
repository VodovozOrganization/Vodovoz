using System;
using Vodovoz.Settings.Counterparty;

namespace Vodovoz.Settings.Database.Counterparty
{
	public class DebtorsSettings : IDebtorsSettings
	{
		private readonly ISettingsController _settingsController;
		private readonly string _debtNotificationWorkerIsDisabledName = "DebtNotificationWorkerIsDisabled";
		public DebtorsSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int GetSuspendedCounterpartyId => _settingsController.GetIntValue("HideSuspendedCounterparty");
		public int GetCancellationCounterpartyId => _settingsController.GetIntValue("HideCancellationCounterparty");

		public bool DebtNotificationWorkerIsDisabled
		{
			get => _settingsController.GetValue<bool>(_debtNotificationWorkerIsDisabledName);

			set => _settingsController.CreateOrUpdateSetting(_debtNotificationWorkerIsDisabledName, value.ToString(), TimeSpan.FromSeconds(5));
		}
	}
}
