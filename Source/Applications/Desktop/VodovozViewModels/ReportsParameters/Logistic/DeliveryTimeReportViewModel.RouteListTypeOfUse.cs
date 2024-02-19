using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.ReportsParameters.Logistic
{
	public partial class DeliveryTimeReportViewModel
	{
		public enum RouteListTypeOfUse
		{
			[Display(Name = "Доставка")]
			Delivery,
			[Display(Name = "СЦ")]
			ServiceCenter,
			[Display(Name = "Фуры")]
			Trucks,
			[Display(Name = "Складская логистика")]
			StorageLogistics
		}
	}
}
