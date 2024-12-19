using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.WageCalculation
{
	/// <summary>
	/// Типы ставок для расчёта ЗП
	/// </summary>
	public enum WageRateTypes
	{
		[Display(Name = "За мобильную связь")]
		PhoneCompensation,
		[Display(Name = "За полную бутыль 19л")]
		Bottle19L,
		[Display(Name = "За пустую бутыль 19л")]
		EmptyBottle19L,
		[Display(Name = "За полную бутыль 19л в крупном заказе")]
		Bottle19LInBigOrder,
		[Display(Name = "За пустую бутыль 19л в крупном заказе")]
		EmptyBottle19LInBigOrder,
		[Display(Name = "Кол-во 19л бутылей для большого заказа")]
		MinBottlesQtyInBigOrder,
		[Display(Name = "За бутыль 6л")]
		Bottle6L,
		[Display(Name = "За упаковку в 36 бутылей объёмом 0.6л")]//пока 1 уп = 36 бут.
		PackOfBottles600ml,
		[Display(Name = "За бутыль 1.5л")]
		Bottle1500ml,
		[Display(Name = "За бутыль 0.5л")]
		Bottle500ml,
		[Display(Name = "За наличие оборудования или единицу оборудования")]
		Equipment,
		[Display(Name = "За 1 адрес в своем районе")]
		Address,
		[Display(Name = "За 1 адрес не в своем районе")]
		ForeignAddress,
		[Display(Name = "За расторжение")]
		ContractCancelation,
		[Display(Name = "Услуга экспресс доставки")]
		FastDelivery,
		[Display(Name = "Услуга экспресс доставки (с опозданием)")]
		FastDeliveryWithLate
	}
}
