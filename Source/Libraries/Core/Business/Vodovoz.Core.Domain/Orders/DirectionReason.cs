using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	public enum DirectionReason
	{
		[Display(Name = "")]
		None,
		[Display(Name = "Аренда")]
		Rent,
		[Display(Name = "Ремонт")]
		Repair,
		[Display(Name = "Санобработка")]
		Cleaning,
		[Display(Name = "Ремонт и санобработка")]
		RepairAndCleaning,
		[Display(Name = "Акция \"Трейд-Ин\"")]
		TradeIn,
		[Display(Name = "Подарок клиента")]
		ClientGift,
	}
}
