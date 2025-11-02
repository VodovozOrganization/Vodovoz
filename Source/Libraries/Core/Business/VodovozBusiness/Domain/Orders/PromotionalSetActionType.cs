using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum PromotionalSetActionType
	{
		[Display(Name = "Фиксированная цена")] FixedPrice
	}
}
