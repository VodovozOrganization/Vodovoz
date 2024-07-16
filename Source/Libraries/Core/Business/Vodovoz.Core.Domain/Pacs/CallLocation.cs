using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	public enum CallLocation
	{
		[Display(Name = "Голосовое меню")]
		Ivr,

		[Display(Name = "Очередь дозвона на группу")]
		Queue,

		[Display(Name = "Сотрудник")]
		Abonent
	}
}
