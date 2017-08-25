using System;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class CalculatedOrder
	{
		public Order Order;

		public int Bootles;
		public double Weight;
		public double Volume;

		public LogisticsArea District;

		public CalculatedOrder(Order order, LogisticsArea district, bool notCalculate = false)
		{
			Order = order;
			District = district;

			if(notCalculate)
				return;
			
			Bootles = order.OrderItems.Where(x => x.Nomenclature.Category == Domain.Goods.NomenclatureCategory.water)
							 .Sum(x => x.Count);
			Weight = order.OrderItems.Sum(x => x.Nomenclature.Weight * x.Count);

			Volume = order.OrderItems.Sum(x => x.Nomenclature.Volume * x.Count);
		}
	}
}
