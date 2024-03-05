using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	public enum OperatorStateType
	{
		[Display(Name = "Новый")]
		New,

		[Display(Name = "Подключен")]
		Connected,

		[Display(Name = "Ожидание")]
		WaitingForCall,

		[Display(Name = "Разговор")]
		Talk,

		[Display(Name = "Перерыв")]
		Break,

		[Display(Name = "Отключен")]
		Disconnected
	}
}
