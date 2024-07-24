using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum CounterpartyFrom
	{
		[Display(Name = "Мобильное приложение")]
		MobileApp = 54,
		[Display(Name = "Сайт")]
		WebSite = 55
	}
}
