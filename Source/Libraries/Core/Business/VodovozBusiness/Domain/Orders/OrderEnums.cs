using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{

	public enum DiscountUnits
	{
		[Display(Name = "₽")]
		money,
		[Display(Name = "%")]
		percent
	}

	/// <summary>
	/// Используется для заполнения комбобоксов
	/// </summary>
	public enum OrderDateType
	{
		[Display(Name = "Дата создания")]
		CreationDate,
		[Display(Name = "Дата доставки")]
		DeliveryDate
	}
}
