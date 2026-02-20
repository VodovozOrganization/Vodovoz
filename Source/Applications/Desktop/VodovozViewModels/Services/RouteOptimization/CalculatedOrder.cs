using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.Services.RouteOptimization
{
	/// <summary>
	/// Класс используется для указания адресов маршрута и хранения рассчитанных
	/// на первоначальном этапе оптимизации итоговых значений заказа. Таких как 
	/// количество бутылей, вес, объем.
	/// </summary>
	public class CalculatedOrder
	{
		public Order Order { get; set; }

		/// <summary>
		/// Ссылка на существующий маршрутный лист, для механизма достраивания маршрутов. См. README.md
		/// </summary>
		public RouteList ExistRoute { get; set; }

		public int Bottles { get; set; }
		public decimal Weight { get; set; }
		public decimal Volume { get; set; }

		public District District { get; set; }

		private GeoGroup _shippingBase;
		public GeoGroup ShippingBase
		{
			get => _shippingBase ?? Order?.DeliveryPoint.District?.GeographicGroup;
			set => _shippingBase = value;
		}

		public CalculatedOrder(Order order, District district, bool notCalculate = false, RouteList existRoute = null)
		{
			Order = order;
			District = district;
			ExistRoute = existRoute;

			if(notCalculate)
			{
				return;
			}

			Bottles = (int)order.OrderItems.Where(
				x => x.Nomenclature.Category == NomenclatureCategory.water
								&& x.Nomenclature.IsWater19L).Sum(x => x.Count);

			Weight = order.OrderItems.Sum(x => x.Nomenclature.Weight * x.Count)
						  + order.OrderEquipments.Where(x => x.Direction == Direction.Deliver)
												 .Sum(x => x.Nomenclature.Weight * x.Count);

			Volume = order.OrderItems.Sum(x => x.Nomenclature.Volume * x.Count)
						  + order.OrderEquipments.Where(x => x.Direction == Direction.Deliver)
												 .Sum(x => x.Nomenclature.Volume * x.Count);
		}
	}
}
