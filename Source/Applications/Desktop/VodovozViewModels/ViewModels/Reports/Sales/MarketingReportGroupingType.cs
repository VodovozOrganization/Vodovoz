using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public enum MarketingReportGroupingType
	{
		[Display(Name = "Все")]
		All,

		[Display(Name = "Категория ABC_XYZ")]
		AbcCategory,

		[Display(Name = "Автор заказа")]
		OrderAuthor
	}
}
