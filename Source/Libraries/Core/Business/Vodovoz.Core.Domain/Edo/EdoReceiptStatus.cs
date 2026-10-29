using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum EdoReceiptStatus
	{
		[Display(Name = "Распределение")]
		New,
		[Display(Name = "Забор кодов")]
		SavedToPool,
		[Display(Name = "Трансфер")]
		Transfering,
		[Display(Name = "Отправляется")]
		Sending,
		[Display(Name = "Отправлен")]
		Sent,
		[Display(Name = "Завершен")]
		Completed
	}
}
