using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	public enum Direction
	{
		[Display(Name = "Доставить")] Deliver,
		[Display(Name = "Забрать")] PickUp
	}
}
