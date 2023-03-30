using System.Net;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Models.CashReceipts.DTO
{
	public class ReceiptSendData
    {
        public CashReceipt CashReceipt { get; set; }

		public FiscalDocument FiscalDocument { get; set; }
	}
}
