using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	public enum CallStatus
	{
		[Display(Name = "Дозвон")]
		Appeared,

		[Display(Name = "Соединен")]
		Connected,

		[Display(Name = "На удержании")]
		OnHold,

		[Display(Name = "Переведен")]
		Transfered,

		[Display(Name = "Завершен")]
		Disconnected
	}
}
