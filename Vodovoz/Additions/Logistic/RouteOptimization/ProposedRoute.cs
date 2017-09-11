using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class ProposedRoute
	{
		public List<ProposedRoutePoint> Orders = new List<ProposedRoutePoint>();
		public AtWorkDriver Driver;
		public DeliveryShift Shift;

		public RouteList RealRoute;

		public long RouteCost;

		public Car Car {
			get {
				return Driver.Car;
			}
		}

		public ProposedRoute(AtWorkDriver driver, DeliveryShift shift)
		{
			Driver = driver;
			Shift = shift;
		}

		public ProposedRoute(PossibleTrip trip)
		{
			Driver = trip?.Driver;
			Shift = trip?.Shift;
		}

		/// <summary>
		/// Метод берет последовательность доставки из построенного маршрута и переносит его в маршрутный лист.
		/// Переносится только последовательность адресов. Никакие адреса не добавляются и не удаляются.
		/// Метод нужен для перестройки с учетов времени уже имеющегося МЛ.
		/// </summary>
		public void UpdateAddressOrderInRealRoute(RouteList updatedRoute)
		{
			for(int i = 0; i < updatedRoute.ObservableAddresses.Count; i++) {
				var address = updatedRoute.ObservableAddresses[i];
				if(i < Orders.Count) {
					if(Orders[i].Order.Id != updatedRoute.ObservableAddresses[i].Order.Id) {
						address = updatedRoute.ObservableAddresses.First(x => x.Order.Id == Orders[i].Order.Id);
						updatedRoute.ObservableAddresses.Remove(address);
						updatedRoute.ObservableAddresses.Insert(i, address);
					}
					address.PlanTimeStart = Orders[i].ProposedTimeStart;
					address.PlanTimeEnd = Orders[i].ProposedTimeEnd;
				} else {
					address.PlanTimeStart = null;
					address.PlanTimeEnd = null;
				}
			}
		}
	}

	public class ProposedRoutePoint
	{
		public TimeSpan ProposedTimeStart;
		public TimeSpan ProposedTimeEnd;
		public Order Order;

		public string DebugMaxMin;

		public ProposedRoutePoint(TimeSpan timeStart, TimeSpan timeEnd, Order order)
		{
			ProposedTimeStart = timeStart;
			ProposedTimeEnd = timeEnd;
			Order = order;
		}
	}
}
