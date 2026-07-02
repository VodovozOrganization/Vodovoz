using System;
using Vodovoz.Settings.Edo;

namespace Vodovoz.Settings.Database.Edo
{
	public class EdoReceiptSettings : IEdoReceiptSettings
	{
		private readonly ISettingsController _settingsController;

		public EdoReceiptSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string EdoReceiptApiUrl => _settingsController
			.GetStringValue("edo.receipt.api_url");

		public int IndustryRequisiteRegulatoryDocumentId => _settingsController
			.GetIntValue("edo.receipt.industry_requisite_regulatory_document_id");

		public int MaxCodesInReceiptCount => _settingsController
			.GetIntValue("edo.receipt.max_codes_in_receipt_count");

		public TimeSpan ReceiptSendPauseStartTime => _settingsController
			.GetValue<TimeSpan>("edo.receipt.send_pause_start_time");

		public TimeSpan ReceiptSendPauseEndTime => _settingsController
			.GetValue<TimeSpan>("edo.receipt.send_pause_end_time");
	}
}
