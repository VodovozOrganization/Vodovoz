using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash.CashTransfer
{
	public enum CashTransferDocumentStatuses
	{
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Отправлен")]
		Sent,
		[Display(Name = "Получен")]
		Received
	}
}
