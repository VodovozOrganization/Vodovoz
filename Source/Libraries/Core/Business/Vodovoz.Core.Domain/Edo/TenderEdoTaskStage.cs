using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Этапы задачи отправки ЭДО документов по Тендеру
	/// </summary>
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
		Completed,
		[Display(Name = "Коды выгружены вручную")]
		ManualUploaded
	}
}
