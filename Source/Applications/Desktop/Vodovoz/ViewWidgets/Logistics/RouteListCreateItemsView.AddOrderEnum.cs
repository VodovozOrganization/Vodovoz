using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	public partial class RouteListCreateItemsView
	{
		public enum AddOrderEnum
		{
			[Display(Name = "Выбрать заказы...")] AddOrders,
			[Display(Name = "Все заказы для логистического района")] AddAllForRegion
		}
	}
}
