using System;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class CalculatedOrder
	{
		public Order Order;

		public int Bootles;
		public double Weight;
		public double Volume;

		public CalculatedOrder(Order order)
		{
			Order = order;
			Bootles = order.OrderItems.Where(x => x.Nomenclature.Category == Domain.Goods.NomenclatureCategory.water)
							 .Sum(x => x.Count);
			Weight = order.OrderItems.Sum(x => x.Nomenclature.Weight * x.Count);

			Volume = order.OrderItems.Sum(x => x.Nomenclature.Volume * x.Count);
		}
	}
}
