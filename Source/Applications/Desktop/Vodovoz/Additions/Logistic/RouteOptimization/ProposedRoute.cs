using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using Vodovoz.Application.Services.Logistics;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	/// <summary>
	/// Предложенный системой оптимизации маршрут.
	/// </summary>
	public class ProposedRoute : IProposedRoute
	{
		private readonly IInteractiveService _interactiveService;
		public List<ProposedRoutePoint> Orders = new List<ProposedRoutePoint>();
		public PossibleTrip Trip;

		public RouteList RealRoute;

		/// <summary>
		/// Балы полученные при построении этого маршрута.
		/// </summary>
		public long RouteCost;

		public ProposedRoute(PossibleTrip trip, IInteractiveService interactiveService)
		{
			Trip = trip;
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		/// <summary>
		/// Метод берет последовательность доставки из предложенного маршрута и переносит его в маршрутный лист.
		/// Переносится только последовательность адресов. Никакие адреса не добавляются и не удаляются.
		/// Метод нужен для перестройки с учетов времени уже имеющегося МЛ.
		/// </summary>
		public void UpdateAddressOrderInRealRoute(RouteList updatedRoute)
		{
			if(updatedRoute.Status > RouteListStatus.InLoading)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, 
					$"Была выполнена попытка перестроить маршрут {updatedRoute.Id} после того, как он отгружен. Проверьте фильтр \"Показать уехавшие\".");

				return;
			}

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

	/// <summary>
	/// Адреса доставки в имеющемся маршруте. Помимо непосредственно заказа, 
	/// возвращают еще и рассчетное время приезда на адрес.
	/// </summary>
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
