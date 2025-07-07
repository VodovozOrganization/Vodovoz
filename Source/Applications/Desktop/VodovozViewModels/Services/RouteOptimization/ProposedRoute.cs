using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Services.RouteOptimization
{
	/// <summary>
	/// Предложенный системой оптимизации маршрут.
	/// </summary>
	public class ProposedRoute
	{
		public ProposedRoute(PossibleTrip trip)
		{
			Trip = trip;
		}

		/// <summary>
		/// Балы полученные при построении этого маршрута.
		/// </summary>
		public long RouteCost { get; set; }

		public List<ProposedRoutePoint> Orders { get; } = new List<ProposedRoutePoint>();
		
		public PossibleTrip Trip { get; }

		public RouteList RealRoute { get; set; }

		/// <summary>
		/// Метод берет последовательность доставки из предложенного маршрута и переносит его в маршрутный лист.
		/// Переносится только последовательность адресов. Никакие адреса не добавляются и не удаляются.
		/// Метод нужен для перестройки с учетов времени уже имеющегося МЛ.
		/// </summary>
		public void UpdateAddressOrderInRealRoute(RouteList updatedRoute)
		{
			if(updatedRoute.Status > RouteListStatus.InLoading)
			{
				throw new InvalidOperationException(
					$"Была выполнена попытка перестроить маршрут {updatedRoute.Id} после того, как он отгружен. Проверьте фильтр \"Показать уехавшие\".");
			}

			for(int i = 0; i < updatedRoute.ObservableAddresses.Count; i++)
			{
				var address = updatedRoute.ObservableAddresses[i];
				if(i < Orders.Count)
				{
					if(Orders[i].Order.Id != updatedRoute.ObservableAddresses[i].Order.Id)
					{
						address = updatedRoute.ObservableAddresses.First(x => x.Order.Id == Orders[i].Order.Id);
						updatedRoute.ObservableAddresses.Remove(address);
						updatedRoute.ObservableAddresses.Insert(i, address);
					}
					address.PlanTimeStart = Orders[i].ProposedTimeStart;
					address.PlanTimeEnd = Orders[i].ProposedTimeEnd;
				}
				else
				{
					address.PlanTimeStart = null;
					address.PlanTimeEnd = null;
				}
			}
		}
	}
}
