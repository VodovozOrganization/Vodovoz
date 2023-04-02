using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Models.CashReceipts.DTO
{
	public class ReceiptSendResult
	{
		public CashReceipt CashReceipt { get; set; }
		public FiscalizationResult FiscalizationResult { get; set; }

	}
}
