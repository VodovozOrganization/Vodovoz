using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum EdoReceiptStatus
	{
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Сохранено в пул")]
		SavedToPool,
		[Display(Name = "Трансфер")]
		Transfering,
		[Display(Name = "Отправка")]
		Sending,
		[Display(Name = "Отправлен")]
		Sent,
		[Display(Name = "Завершен")]
		Completed
	}
}
