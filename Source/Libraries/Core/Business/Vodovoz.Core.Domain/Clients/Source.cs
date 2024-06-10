using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	public enum Source
	{
		[Display(Name = "Мобильное приложение")]
		MobileApp = 54,
		[Display(Name = "Сайт ВВ")]
		VodovozWebSite = 55,
		[Display(Name = "Сайт Кулер Сэйл")]
		KulerSaleWebSite = 56
	}
}
