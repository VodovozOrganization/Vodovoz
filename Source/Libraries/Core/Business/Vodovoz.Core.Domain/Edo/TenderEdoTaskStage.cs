using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum TenderEdoTaskStage
	{
		[Display(Name = "Новый")]
		New,
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
