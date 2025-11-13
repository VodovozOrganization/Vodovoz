using System;
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

		public int TransferTaskRequestsWaitingTimeoutMinute => _settingsController
			.GetIntValue("edo.transfer.requests_waiting_timeout_minutes");

		public int TransferTaskRequestsWaitingTimeoutCheckIntervalSecond => _settingsController
			.GetIntValue("edo.transfer.requests_waiting_timeout_check_interval_seconds");

		public TimeSpan WaitingTransfersUpdateInterval => _settingsController
			.GetValue<TimeSpan>("edo.transfer.waiting_transfers_update_interval");

		public TimeSpan ClosingDocumentsOrdersUpdSendInterval => _settingsController
			.GetValue<TimeSpan>("edo.transfer.closing_documents_orders_upd_send_interval");

		public int ClosingDocumentsOrdersUpdSendMaxDaysFromDeliveryDate => _settingsController
			.GetIntValue("edo.transfer.closing_docs_upd_send_max_days_from_delivery");

		public int MinCodesCountForStartTransfer => _settingsController
			.GetIntValue("edo.transfer.min_codes_count_for_start_transfer");

		public TimeSpan TransferTimeoutInterval => _settingsController
			.GetValue<TimeSpan>("edo.transfer.timeout_interval");

		public int AdditionalPurchasePricePrecentForTransfer
		{
			get
			{
				var percentFromBase = _settingsController.GetIntValue("edo.transfer.additional_purchase_price_percent");
				if(percentFromBase > 100)
				{
					return 100;
				}

				if(percentFromBase < 0)
				{
					return 0;
				}
				return percentFromBase;
			}
		}
	}
}
