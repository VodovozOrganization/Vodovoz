using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public enum MarketingReportDateType
	{
		[Display(Name = "По дате доставки")]
		DeliveryDate,

		[Display(Name = "По дате создания заказа")]
		CreationDate
	}
}
