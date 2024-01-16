using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class DeliveryTimeReport
	{
		private enum RouteListTypeOfUse
		{
			[Display(Name = "Доставка")]
			Delivery,
			[Display(Name = "СЦ")]
			ServiceCenter,
			[Display(Name = "Фуры")]
			CompanyTrucks,
			[Display(Name = "Складская логистика")]
			StorageLogistics
		}
	}
}
