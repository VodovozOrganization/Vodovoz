using System;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{

	/// <summary>
	/// Класс используется для указания адресов маршрута и хранения рассчитанных
	/// на первоначальном этапе оптимизации итоговых значений заказа. Таких как 
	/// количество бутылей, вес, объем.
	/// </summary>
	public class CalculatedOrder
	{
		public Order Order;

		/// <summary>
		/// Ссылка на существующий маршрутный лист, для механизма достраивания маршрутов. См. README.md
		/// </summary>
		public RouteList ExistRoute;

		public int Bootles;
		public double Weight;
		public double Volume;

		public LogisticsArea District;

		public CalculatedOrder(Order order, LogisticsArea district, bool notCalculate = false, RouteList existRoute = null)
		{
			Order = order;
			District = district;
			ExistRoute = existRoute;

			if(notCalculate)
				return;
			
			Bootles = order.OrderItems.Where(x => x.Nomenclature.Category == Domain.Goods.NomenclatureCategory.water)
							 .Sum(x => x.Count);
			Weight = order.OrderItems.Sum(x => x.Nomenclature.Weight * x.Count);

			Volume = order.OrderItems.Sum(x => x.Nomenclature.Volume * x.Count);
		}
	}
}
