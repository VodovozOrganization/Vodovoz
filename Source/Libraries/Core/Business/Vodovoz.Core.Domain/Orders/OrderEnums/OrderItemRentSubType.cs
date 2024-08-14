using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum OrderItemRentSubType
	{
		[Display(Name = "Нет аренды")]
		None,

		[Display(Name = "Услуга аренды")]
		RentServiceItem,

		[Display(Name = "Залог за аренду")]
		RentDepositItem
	}
}
