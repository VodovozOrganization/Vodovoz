using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.ReportsParameters.Sales
{
	public enum IncomeReportType
	{
		[Display(Name = "Общий отчет")]
		Сommon,
		[Display(Name = "По МЛ")]
		ByRouteList,
		[Display(Name = "По Самовывозу")]
		BySelfDelivery
	}
}
