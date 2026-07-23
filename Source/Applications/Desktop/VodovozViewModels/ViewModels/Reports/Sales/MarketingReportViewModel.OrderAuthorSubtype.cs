using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class MarketingReportViewModel
	{
		public enum OrderAuthorSubtype
		{
			[Display(Name = "Сайт ВВ")]
			Site,

			[Display(Name = "Мобильное приложение")]
			MobileApp,

			[Display(Name = "Подразделение автора")]
			Subdivision
		}
	}
}
