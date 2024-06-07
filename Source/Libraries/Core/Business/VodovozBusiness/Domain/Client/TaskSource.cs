using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum TaskSource
	{
		[Display(Name = "Боковая панель заказа")]
		OrderPanel,
		[Display(Name = "Автоматическое создание(из заказа)")]
		AutoFromOrder,
		[Display(Name = "Массовое создание(из журнала задолженостей)")]
		MassCreation,
		[Display(Name = "Создана вручную")]
		Handmade
	}
}
