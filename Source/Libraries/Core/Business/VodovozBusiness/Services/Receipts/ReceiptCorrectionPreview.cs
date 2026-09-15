using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Receipts;

namespace VodovozBusiness.Services.Receipts
{
	/// <summary>
	/// Результат проверки: будет ли при сохранении заказа запущена корректировка чека.
	/// </summary>
	public class ReceiptCorrectionPreview
	{
		public static ReceiptCorrectionPreview None { get; } = new ReceiptCorrectionPreview();

		public bool WillStartProcess { get; set; }

		public ReceiptCorrectionScenarioType ScenarioType { get; set; }

		public IReadOnlyList<FiscalDocumentType> PlannedDocumentTypes { get; set; } = new List<FiscalDocumentType>();
	}
}
