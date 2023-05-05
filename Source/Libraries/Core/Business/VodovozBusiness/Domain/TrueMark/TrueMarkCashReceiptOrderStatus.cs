using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.TrueMark
{
	public enum TrueMarkCashReceiptOrderStatus
	{
		[Display(Name = "Новый")]
		New,

		[Display(Name = "Готово к отправке")]
		ReadyToSend,

		[Display(Name = "Ошибка кода")]
		CodeError,

		[Display(Name = "Ошибка отправки")]
		ReceiptSendError,

		[Display(Name = "Чек не требуется")]
		ReceiptNotNeeded,

		[Display(Name = "Отправлен")]
		Sended,

		[Display(Name = "Дубликат по сумме")]
		DuplicateSum
	}
}
