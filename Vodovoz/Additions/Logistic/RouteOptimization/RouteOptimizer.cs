using System.Collections.Generic;
using System.Linq;
using Gtk;
using NetTopologySuite.Geometries;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class RouteOptimizer
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Настройки оптимизации
		public static double UnlikeDistrictCost = 3;

		#endregion

		public IList<RouteList> Routes;
		public IList<Domain.Orders.Order> Orders;
		public IList<AtWorkDriver> Drivers;
		public IList<AtWorkForwarder> Forwarders;

		public ProposedPlan BestPlan;

		public ProgressBar OrdersProgress;

		public IUnitOfWork UoW;

		public RouteOptimizer()
		{
		}

		public void CreateRoutes()
		{
			logger.Info("Разбираем заказы по районам...");
			MainClass.MainWin.ProgressStart(3);
			var areas = UoW.GetAll<LogisticsArea>().ToList();
			List<DistrictInfo> districts = new List<DistrictInfo>();

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

			var allDrivers = Drivers.Where(x => x.Car != null).OrderBy(x => x.Employee.TripPriority).ToList();

			ProposedPlan.BestFinishedPlan = null;
			ProposedPlan.BestFinishedCost = double.MaxValue;
			ProposedPlan.BestNotFinishedPlan = null;
			var startPaln = new ProposedPlan();
			startPaln.RemainDrivers = allDrivers;
			startPaln.RemainOrders = districts.Select(x => new FreeOrders(x, x.OrdersInDistrict)).ToList();
			OrdersProgress.Adjustment.Upper = ProposedPlan.BestNotFinishedCount = startPaln.FreeOrdersCount;
			RecursiveSearch(startPaln);
			MainClass.MainWin.ProgressAdd();
			BestPlan = ProposedPlan.BestFinishedPlan ?? ProposedPlan.BestNotFinishedPlan;
			if(BestPlan != null)
				logger.Info($"Предложено {BestPlan.Routes.Count} маршрутов.");
		}

		void RecursiveSearch(ProposedPlan curPlan)
		{
			//OrdersProgress.Adjustment.Value = OrdersProgress.Adjustment.Upper - curPlan.FreeOrdersCount;
			OrdersProgress.Text = string.Join(":", curPlan.DebugLevel);
			curPlan.DebugLevel.Add(0);

			QSMain.WaitRedraw();
			if(curPlan.CurRoute == null)
			{
				var driver = curPlan.RemainDrivers.First();
				curPlan.RemainDrivers.Remove(driver);
				curPlan.CurRoute = new ProposedRoute(driver);
				curPlan.Routes.Add(curPlan.CurRoute);
				logger.Debug("Новый водитель.");
			}

			var prioritedDistricts = curPlan.CurRoute.Driver.Employee.Districts
							   .Select(x => curPlan.RemainOrders.FirstOrDefault(d => d.District.District.Id == x.District.Id))
						   .Where(x => x != null)
						   .ToList();

			double districtCost = 0;
			bool notAdded = true;
			foreach(var district in prioritedDistricts) {
				foreach(var order in district.Orders) {
					curPlan.DebugLevel[curPlan.DebugLevel.Count-1]++;
					if(curPlan.CurRoute.CanAdd(order)) {
						notAdded = false;
						var newPlan = curPlan.Clone();
						newPlan.OrderTaked(order);
						newPlan.PlanCost += newPlan.CurRoute.AddOrder(order);
						newPlan.PlanCost += districtCost;
						var totalRemain = newPlan.FreeOrdersCount;
						if(totalRemain == 0)
						{
							newPlan.PlanCost += DistanceCalculator.GetDistanceToBase(order.DeliveryPoint);
						}

						if(newPlan.PlanCost >= ProposedPlan.BestFinishedCost)
							continue;

						if(totalRemain < ProposedPlan.BestNotFinishedCount)
						{
							ProposedPlan.BestNotFinishedCount = totalRemain;
							ProposedPlan.BestNotFinishedPlan = newPlan;
							OrdersProgress.Adjustment.Value = OrdersProgress.Adjustment.Upper - totalRemain;
							OrdersProgress.Text = RusNumber.FormatCase(totalRemain, "Остался {0} заказ", "Осталось {0} заказа", "Осталось {0} заказов");
							QSMain.WaitRedraw();
						}

						if(totalRemain == 0) {
							OrdersProgress.Adjustment.Value = OrdersProgress.Adjustment.Upper - totalRemain;
							OrdersProgress.Text = "Все развезем. Ищем оптимальные варианты...";
							MainClass.MainWin.ProgressUpdate(2);
							ProposedPlan.BestFinishedCost = newPlan.PlanCost;
							ProposedPlan.BestFinishedPlan = newPlan;
							logger.Info($"Найден новый вариант общей стоимостью в {newPlan.PlanCost} очков.");
							continue;
						}
						logger.Debug("Следующий заказ водитель.");
						RecursiveSearch(newPlan);
					}
				}
				districtCost += UnlikeDistrictCost;
			}
			if(notAdded)
			{
				if(curPlan.CurRoute.Orders.Count == 0)
				{
					curPlan.Routes.Remove(curPlan.CurRoute);
				}
				else
				{
					curPlan.PlanCost += DistanceCalculator.GetDistanceToBase(curPlan.CurRoute.Orders.Last().DeliveryPoint);
				}
					
				if(curPlan.RemainDrivers.Count > 0)
				{
					//var newPlan = curPlan.Clone();
					curPlan.CurRoute = null;
					RecursiveSearch(curPlan);
				}
			}
		}
	}
}
