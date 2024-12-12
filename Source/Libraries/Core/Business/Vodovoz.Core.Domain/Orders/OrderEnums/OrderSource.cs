using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum OrderSource
	{
		[Display(Name = "Диспетчер")]
		VodovozApp,
		[Display(Name = "Интернет магазин")]
		OnlineStore,
		[Display(Name = "Мобильное приложение")]
		MobileApp
	}
}
