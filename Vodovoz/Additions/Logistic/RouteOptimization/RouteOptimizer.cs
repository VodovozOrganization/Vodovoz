using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.ConstraintSolver;
using Gtk;
using NetTopologySuite.Geometries;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Logistic;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class RouteOptimizer
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Настройки оптимизации
		public static long UnlikeDistrictPenalty = 100000; //Штраф за поездку в отсутствующий в списке район
		public static long DistrictPriorityPenalty = 1000; //Штраф за каждый шаг приоритета к каждому адресу, в менее приоритеном районе
		public static long DriverPriorityPenalty = 20000; //Штраф каждому менее приоритетному водителю, на единицу приоритета, за выход в маршрут.
		public static long MaxDistanceAddressPenalty = 300000; //Штраф за не отвезенный заказ. Или максимальное расстояние на которое имеет смысл ехать.

		#endregion

		public IList<RouteList> Routes;
		public IList<Domain.Orders.Order> Orders;
		public IList<AtWorkDriver> Drivers;
		public IList<AtWorkForwarder> Forwarders;

		private CalculatedOrder[] Nodes;
		private IDistanceCalculator distanceCalculator;

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
			logger.Info("Подготавливаем заказы...");
			PerformanceHelper.StartMeasurement($"Строим оптимальные маршруты");
			MainClass.MainWin.ProgressStart(4);

			//Сортируем в обратном порядке потому что алгоритм отдает предпочтение водителям с конца.
			var allDrivers = Drivers.Where(x => x.Car != null).OrderByDescending(x => x.Employee.TripPriority).ToArray();
			if(allDrivers.Length == 0) {
				logger.Error("Для построения маршрутов, нет водителей.");
				return;
			}

			var areas = UoW.GetAll<LogisticsArea>().ToList();
			List<LogisticsArea> unusedDistricts = new List<LogisticsArea>();
			List<CalculatedOrder> calculatedOrders = new List<CalculatedOrder>();

			foreach(var order in Orders)
			{
				if(order.DeliveryPoint.Longitude == null || order.DeliveryPoint.Latitude == null)
					continue;
				var point = new Point((double)order.DeliveryPoint.Latitude.Value, (double)order.DeliveryPoint.Longitude.Value);
				var aria = areas.Find(x => x.Geometry.Contains(point));
				if(aria != null)
				{
					if(allDrivers.SelectMany(x => x.Employee.Districts).Any(x => x.District.Id == aria.Id))
						calculatedOrders.Add(new CalculatedOrder(order, aria));
					else if(!unusedDistricts.Contains(aria))
						unusedDistricts.Add(aria);
				}
			}
			Nodes = calculatedOrders.ToArray();
			if(unusedDistricts.Count > 0)
			{
				logger.Warn("Районы без водителей: {0}", String.Join(", ", unusedDistricts.Select(x => x.Name)));
			}

			distanceCalculator = new DistanceCalculatorSputnik(Nodes.Select(x => x.Order.DeliveryPoint).ToArray(), DebugBuffer);

			MainClass.MainWin.ProgressAdd();
			logger.Info("Развозка по {0} районам.", calculatedOrders.Select(x => x.District).Distinct().Count());
			PerformanceHelper.AddTimePoint(logger, $"Подготовка заказов");

			logger.Info("Создаем модель...");
			RoutingModel routing = new RoutingModel(Nodes.Length + 1, allDrivers.Length, 0);

			for(int ix = 0; ix < allDrivers.Length; ix++)
			{
				routing.SetArcCostEvaluatorOfVehicle(new CallbackDistanceDistrict(Nodes, allDrivers[ix], distanceCalculator), ix);
				routing.SetFixedCostOfVehicle((allDrivers[ix].Employee.TripPriority - 1) * DriverPriorityPenalty, ix);
			}

			var bottlesCapacity = allDrivers.Select(x => (long)x.Car.MaxBottles + 1).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackBottles(Nodes), 0, bottlesCapacity, true, "Bottles" );

			var weightCapacity = allDrivers.Select(x => (long)x.Car.MaxWeight + 1).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackWeight(Nodes), 0, weightCapacity, true, "Weight");

			var volumeCapacity = allDrivers.Select(x => (long)(x.Car.MaxVolume * 1000) + 1).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackVolume(Nodes), 0, volumeCapacity, true, "Volume");

			var addressCapacity = allDrivers.Select(x => (long)(x.Car.MaxRouteAddresses + 1)).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackAddressCount(Nodes.Length), 0, addressCapacity, true, "AddressCount");

			for(int ix = 1; ix < Nodes.Length; ix++)
				routing.AddDisjunction(new int[]{ix}, MaxDistanceAddressPenalty);

			RoutingSearchParameters search_parameters =
			        RoutingModel.DefaultSearchParameters();
			// Setting first solution heuristic (cheapest addition).
			search_parameters.FirstSolutionStrategy =
				                 FirstSolutionStrategy.Types.Value.ParallelCheapestInsertion;
			//			var solver = routing.solver();
			//routing.AddSearchMonitor(new CallbackMonitor(solver, OrdersProgress));
			search_parameters.TimeLimitMs = 30000;
			search_parameters.FingerprintArcCostEvaluators = true;
			search_parameters.OptimizationStep = 100;

			var solver = routing.solver();

			PerformanceHelper.AddTimePoint(logger, $"Настроили оптимизацию");
			MainClass.MainWin.ProgressAdd();
			logger.Info("Закрываем модель...");
			logger.Info("Рассчет расстояний между точками...");
			routing.CloseModelWithParameters(search_parameters);

			var lastSolution = solver.MakeLastSolutionCollector();
			lastSolution.AddObjective(routing.CostVar());
			routing.AddSearchMonitor(lastSolution);
			routing.AddSearchMonitor(new CallbackMonitor(solver, OrdersProgress, DebugBuffer, lastSolution));

			PerformanceHelper.AddTimePoint(logger, $"Закрыли модель");
			logger.Info("Поиск решения...");
			MainClass.MainWin.ProgressAdd();

			Assignment solution = routing.SolveWithParameters(search_parameters);
			PerformanceHelper.AddTimePoint(logger, $"Получили решение.");
			logger.Info("Готово. Заполняем.");
			MainClass.MainWin.ProgressAdd();

			Console.WriteLine("Status = {0}", routing.Status());
			if(solution != null) {
				// Solution cost.
				Console.WriteLine("Cost = {0}", solution.ObjectiveValue());
				for(int route_number = 0; route_number < routing.Vehicles(); route_number++)
				{
					//FIXME Нужно понять, есть ли у водителя маршрут.
					var route = new ProposedRoute(allDrivers[route_number]);
					long first_node = routing.Start(route_number);
					long second_node = solution.Value(routing.NextVar(first_node)); // Пропускаем первый узел, так как это наша база.
					route.RouteCost = routing.GetCost(first_node, second_node, route_number);
					while(!routing.IsEnd(second_node))
					{
						route.Orders.Add(Nodes[second_node - 1].Order);
						first_node = second_node;
						second_node = solution.Value(routing.NextVar(first_node));
						route.RouteCost += routing.GetCost(first_node, second_node, route_number);
					}

					if(route.Orders.Count > 0)
						ProposedRoutes.Add(route);
				}
			}

			MainClass.MainWin.ProgressAdd();

			if(ProposedRoutes.Count > 0)
				logger.Info($"Предложено {ProposedRoutes.Count} маршрутов.");
			PerformanceHelper.Main.PrintAllPoints(logger);
		}
	}
}
