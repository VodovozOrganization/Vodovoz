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

		public int DebtNotificationWorkerIntervalSeconds => _settingsController.GetIntValue("DebtNotificationWorkerIntervalSeconds");

		public int LettersOfClaimTimeoutDays => _settingsController.GetIntValue("DebtorsSettings.LettersOfClaimTimeoutDays");

		public TimeSpan LettersOfClaimWorkerInterval => _settingsController.GetValue<TimeSpan>("DebtorsSettings.LettersOfClaimWorkerInterval");

		public int LettersOfClaimMaxCountPerInterval => _settingsController.GetIntValue("DebtorsSettings.LettersOfClaimMaxCountPerInterval");

		public int LettersOfClaimMaxCountPerDay => _settingsController.GetIntValue("DebtorsSettings.LettersOfClaimMaxCountPerDay"); 

		public int LettersOfClaimResendIntervalDays => _settingsController.GetIntValue("DebtorsSettings.LettersOfClaimResendIntervalDays");

		public string ClaimDocumentCreatedBy => _settingsController.GetValue<string>("ClaimDocument.CreatedBy");

		public string ClaimDocumentCreatorPhone => _settingsController.GetValue<string>("ClaimDocument.CreatorPhone");

		public void SetLettersOfClaimTimeoutDays(int value)
		{
			_settingsController.CreateOrUpdateSetting("DebtorsSettings.LettersOfClaimTimeoutDays}", value.ToString());
		}

		public void SetClaimDocumentCreatedBy(string value)
		{
			_settingsController.CreateOrUpdateSetting("ClaimDocument.CreatedBy}", value);
		}

		public void SetClaimDocumentCreatorPhone(string value)
		{
			_settingsController.CreateOrUpdateSetting("ClaimDocument.CreatorPhone}", value);
		}
	}
}
