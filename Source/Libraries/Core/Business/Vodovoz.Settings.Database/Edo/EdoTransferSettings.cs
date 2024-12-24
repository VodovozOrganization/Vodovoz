using Vodovoz.Settings.Edo;

namespace Vodovoz.Settings.Database.Edo
{
	public class EdoTransferSettings : IEdoTransferSettings
	{
		private readonly ISettingsController _settingsController;

		public EdoTransferSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new System.ArgumentNullException(nameof(settingsController));
		}

		public string TaxcomIntegratorId => _settingsController.GetStringValue("TaxcomIntegratorId");

		public int TransferTaskTimeoutMinute => _settingsController.GetIntValue("edo.transfer.task_timeout_minutes");

		public int TransferTaskTimeoutCheckIntervalSecond => _settingsController.GetIntValue("edo.transfer.task_timeout_check_interval_seconds");

		public int MinCodesCountForStartTransfer => _settingsController.GetIntValue("edo.transfer.min_codes_count_for_start_transfer");
	}
}
