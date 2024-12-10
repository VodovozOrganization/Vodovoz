using System;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Services.RouteOptimization
{
	/// <summary>
	/// Адреса доставки в имеющемся маршруте. Помимо непосредственно заказа, 
	/// возвращают еще и рассчетное время приезда на адрес.
	/// </summary>
	public class ProposedRoutePoint
	{
		public ProposedRoutePoint(TimeSpan timeStart, TimeSpan timeEnd, Order order)
		{
			ProposedTimeStart = timeStart;
			ProposedTimeEnd = timeEnd;
			Order = order;
		}

		public TimeSpan ProposedTimeStart { get; set; }

		public TimeSpan ProposedTimeEnd { get; set; }

		public Order Order { get; set; }

		public string DebugMaxMin { get; set; }
	}
}
