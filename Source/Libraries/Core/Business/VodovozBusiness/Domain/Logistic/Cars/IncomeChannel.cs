using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Cars
{
	public enum IncomeChannel
	{
		[Display(Name = "Пустой ( базово)")]
		None,
		[Display(Name = "Куплен - Новый")]
		PurchasedNew,
		[Display(Name = "Куплен - БУ")]
		PurchasedUsed,
		[Display(Name = "ТС водителя")]
		DriverProperty
	}
}
