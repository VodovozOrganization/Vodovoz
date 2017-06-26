using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class RouteOptimizer
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public IList<RouteList> Routes;
		public IList<Domain.Orders.Order> Orders;
		public IList<AtWorkDriver> Drivers;
		public IList<AtWorkForwarder> Forwarders;

		private List<DistrictInfo> districts = new List<DistrictInfo>();
		public List<ProposedRoute> ProposedRoutes = new List<ProposedRoute>();

		public IUnitOfWork UoW;

		public RouteOptimizer()
		{
		}

		public void CreateRoutes()
		{
			logger.Info("Разбираем заказы по районам...");
			MainClass.MainWin.ProgressStart(2);
			var areas = UoW.GetAll<LogisticsArea>().ToList();
			foreach(var order in Orders)
			{
				if(order.DeliveryPoint.Longitude == null || order.DeliveryPoint.Latitude == null)
					continue;
				var point = new Point((double)order.DeliveryPoint.Latitude.Value, (double)order.DeliveryPoint.Longitude.Value);
				var aria = areas.Find(x => x.Geometry.Contains(point));
				if(aria != null)
				{
					var district = districts.FirstOrDefault(x => x.District.Id == aria.Id);
					if(district == null)
					{
						district = new DistrictInfo(aria);
						districts.Add(district);
					}
					district.OrdersInDistrict.Add(order);
				}
			}
			districts.ForEach(x => x.FreeOrders = x.OrdersInDistrict.ToList());

			MainClass.MainWin.ProgressAdd();
			logger.Info($"Развозка по {districts.Count} районам.");

			foreach(var driver in Drivers.OrderBy(x => x.Employee.TripPriority))
			{
				if(driver.Car == null)
				{
					logger.Warn($"Водитель {driver.Employee.ShortName} пропущен, так как он без автомобиля.");
					continue;
				}
				var proposed = new ProposedRoute(driver);
				var prioritedDistricts = driver.Employee.Districts
										   .Select(x => districts.FirstOrDefault(d => d.District.Id == x.District.Id))
										   .Where(x => x != null)
										   .ToList();
				foreach(var district in prioritedDistricts)
				{
					foreach(var order in district.FreeOrders.OrderByDescending(x => x.DeliveryPoint.Latitude).ToList())
					{
						if(proposed.CanAdd(order))
						{
							proposed.Orders.Add(order);
							district.FreeOrders.Remove(order);
						}
					}
				}
				if(proposed.Orders.Count > 0)
					ProposedRoutes.Add(proposed);
			}
			MainClass.MainWin.ProgressAdd();
			logger.Info($"Предложено {ProposedRoutes.Count} маршрутов.");
		}
	}
}
