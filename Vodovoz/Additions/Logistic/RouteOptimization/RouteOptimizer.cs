using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.ConstraintSolver;
using Gtk;
using NetTopologySuite.Geometries;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Logistic;

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

		private CalculatedOrder[] Nodes;

		public ProgressBar OrdersProgress;
		public Gtk.TextBuffer DebugBuffer;

		public bool Cancel = false;

		public IUnitOfWork UoW;

		#region Результат
		public List<ProposedRoute> ProposedRoutes = new List<ProposedRoute>();
		#endregion

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

			MainClass.MainWin.ProgressAdd();
			logger.Info($"Развозка по {districts.Count} районам.");

			var allDrivers = Drivers.Where(x => x.Car != null).OrderBy(x => x.Employee.TripPriority).ToArray();
			if(allDrivers.Length == 0)
			{
				logger.Error("Для построения маршрутов, нет водителей.");
				return;
			}

			logger.Info("Подсчитываем товары в заказах...");
			Nodes = districts.SelectMany(x => x.OrdersInDistrict).Select(x => new CalculatedOrder(x)).ToArray();
			MainClass.MainWin.ProgressAdd();

			logger.Info("Настраиваем оптимизацию...");
			RoutingModel routing = new RoutingModel(Nodes.Length + 1, allDrivers.Length, 0);

			routing.SetCost(new CallbackDistance(Nodes));

			// Solve, returns a solution if any (owned by RoutingModel).
			RoutingSearchParameters search_parameters =
			        RoutingModel.DefaultSearchParameters();
			// Setting first solution heuristic (cheapest addition).
			search_parameters.FirstSolutionStrategy =
				                 FirstSolutionStrategy.Types.Value.PathCheapestArc;

			logger.Info("Поиск первого решения...");
			MainClass.MainWin.ProgressAdd();

			Assignment solution = routing.SolveWithParameters(search_parameters);
			Console.WriteLine("Status = {0}", routing.Status());
			if(solution != null) {
				// Solution cost.
				Console.WriteLine("Cost = {0}", solution.ObjectiveValue());
				for(int route_number = 0; route_number < routing.Vehicles(); route_number++)
				{
					//FIXME Нужно понять, есть ли у водителя маршрут.
					var route = new ProposedRoute(allDrivers[route_number]);
					ProposedRoutes.Add(route);
					for(long node = routing.Start(route_number);
						 !routing.IsEnd(node);
						 node = solution.Value(routing.NextVar(node))) {
						if(node == 0)
							continue;
						route.Orders.Add(Nodes[node - 1].Order);
					}
				}
			}

			MainClass.MainWin.ProgressAdd();

			if(ProposedRoutes.Count > 0)
				logger.Info($"Предложено {ProposedRoutes.Count} маршрутов.");
		}

		static DateTime lastRedraw;

		void RecursiveSearch(ProposedPlan curPlan)
		{
			if (Cancel)
				return;

			curPlan.DebugLevel.Add(0);

			if(curPlan.CurRoute == null)
			{
				var driver = curPlan.RemainDrivers.First();
				curPlan.RemainDrivers.Remove(driver);
				curPlan.CurRoute = new ProposedRoute(driver);
				curPlan.Routes.Add(curPlan.CurRoute);
				curPlan.CurRoute.PossibleOrders = curPlan.CurRoute.Driver.Employee.Districts
					.Select(x => curPlan.RemainOrders.FirstOrDefault(d => d.District.District.Id == x.District.Id))
					.Where(x => x != null)
					.Select(x => x.Clone())
					.ToList();
				
				logger.Debug("Новый водитель.");
			}

			double districtCost = 0;
			bool notAdded = true;
			foreach(var district in curPlan.CurRoute.PossibleOrders) {
				foreach(var order in district.Orders.ToList()) {

					//Просто для отображения технической информации.
					curPlan.DebugLevel[curPlan.DebugLevel.Count-1]++;
					if (DateTime.Now.Subtract(lastRedraw).Milliseconds > 200)
					{
						lastRedraw = DateTime.Now;
						//OrdersProgress.Adjustment.Value = OrdersProgress.Adjustment.Upper - curPlan.FreeOrdersCount;
						OrdersProgress.Text = string.Join(":", curPlan.DebugLevel);
						DebugBuffer.Text = String.Format("Район: {0}({1}\\{2}\\{3})\nВодитель: {4}({5})\nМаршрутов: {6}({7})",
														 district.District.District.Name,
														 district.District.OrdersInDistrict.Count,
						                                 curPlan.RemainOrders.First(x => x.District == district.District).Orders.Count,
						                                 district.Orders.Count,
						                                 curPlan.CurRoute.Driver.Employee.ShortName,
						                                 curPlan.CurRoute.Orders.Count,
						                                 curPlan.Routes.Count,
						                                 curPlan.RemainDrivers.Count
														);
						QSMain.WaitRedraw();
					}

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
						RecursiveSearch(newPlan);
					}
					else
					{
						curPlan.CurRoute.RemoveFromPossible(order);
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
