using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	public class OrderEdoRequest : CustomerEdoRequest
	{
		private OrderEntity _order;

		[Display(Name = "Код заказа")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}
	}
}
