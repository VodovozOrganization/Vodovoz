﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.OrTools.ConstraintSolver;
using Gtk;
using NetTopologySuite.Geometries;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Logistic;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class RouteOptimizer : PropertyChangedBase
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Настройки оптимизации
		public static long UnlikeDistrictPenalty = 100000; //Штраф за поездку в отсутствующий в списке район
		public static long DistrictPriorityPenalty = 1000; //Штраф за каждый шаг приоритета к каждому адресу, в менее приоритеном районе
		public static long DriverPriorityPenalty = 20000; //Штраф каждому менее приоритетному водителю, на единицу приоритета, за выход в маршрут.
		public static long MaxDistanceAddressPenalty = 300000; //Штраф за не отвезенный заказ. Или максимальное расстояние на которое имеет смысл ехать.
		public static int MaxBottlesInOrderForLargus = 4; //Максимальное количество бутелей в заказе для ларгусов.
		public static long LargusMaxBottlePenalty = 500000; //Штраф за добавление в лагрус большего количества бутелей. Сейчас установлено больше чем стоимость недоставки заказа.
		#endregion

		public IList<RouteList> Routes;
		public IList<Domain.Orders.Order> Orders;
		public IList<AtWorkDriver> Drivers;
		public IList<AtWorkForwarder> Forwarders;

		private CalculatedOrder[] Nodes;
		private ExtDistanceCalculator distanceCalculator;

		public ProgressBar OrdersProgress;
		public Gtk.TextBuffer DebugBuffer;

		public bool Cancel = false;

		public int MaxTimeSeconds { get; set; } = 30;

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
			var allDrivers = Drivers.Where(x => x.Car != null).OrderByDescending(x => x.PriorityAtDay).ToArray();
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
					if(allDrivers.SelectMany(x => x.Districts).Any(x => x.District.Id == aria.Id))
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

			distanceCalculator = new ExtDistanceCalculator(DistanceProvider.Osrm, Nodes.Select(x => x.Order.DeliveryPoint).ToArray(), DebugBuffer);

#region Нужно только для подсчета предположителного количества расстояний.
			Dictionary<LogisticsArea, int> pointsByDistrict = Nodes.GroupBy(x => x.District).ToDictionary(x => x.Key, y => y.Count());
			var districtPairs = Drivers.SelectMany(diver =>
										   diver.Districts.SelectMany(dis1 =>
			                                                                   diver.Districts//.Where(dis2 => dis2.District.Id >= dis1.District.Id)
			                                                                   .Select(dis2 => new Tuple<LogisticsArea, LogisticsArea>(dis1.District, dis2.District))
																			  )).Distinct().ToList();
			var total = districtPairs.Where(pair => pointsByDistrict.ContainsKey(pair.Item1) && pointsByDistrict.ContainsKey(pair.Item2))
			                         .Sum(pair => pointsByDistrict[pair.Item1] * pointsByDistrict[pair.Item2] * (pair.Item1 == pair.Item2 ? 2 : 1));
			distanceCalculator.ProposeNeedCached = total + Nodes.Length * 2;
#endregion
			MainClass.MainWin.ProgressAdd();
			logger.Info("Развозка по {0} районам.", calculatedOrders.Select(x => x.District).Distinct().Count());
			PerformanceHelper.AddTimePoint(logger, $"Подготовка заказов");

			logger.Info("Создаем модель...");
			RoutingModel routing = new RoutingModel(Nodes.Length + 1, allDrivers.Length, 0);

			int horizon = 24 * 3600;

			routing.AddDimension(new CallbackTime(Nodes, null, distanceCalculator), 3 * 3600, horizon, false, "Time");
			var time_dimension = routing.GetDimensionOrDie("Time");

			var bottlesCapacity = allDrivers.Select(x => (long)x.Car.MaxBottles + 1).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackBottles(Nodes), 0, bottlesCapacity, true, "Bottles" );

			var weightCapacity = allDrivers.Select(x => (long)x.Car.MaxWeight + 1).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackWeight(Nodes), 0, weightCapacity, true, "Weight");

			var volumeCapacity = allDrivers.Select(x => (long)(x.Car.MaxVolume * 1000) + 1).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackVolume(Nodes), 0, volumeCapacity, true, "Volume");

			var addressCapacity = allDrivers.Select(x => (long)(x.Car.MaxRouteAddresses + 1)).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackAddressCount(Nodes.Length), 0, addressCapacity, true, "AddressCount");

			var bottlesDimension = routing.GetDimensionOrDie("Bottles");

			for(int ix = 0; ix < allDrivers.Length; ix++) {
				routing.SetArcCostEvaluatorOfVehicle(new CallbackDistanceDistrict(Nodes, allDrivers[ix], distanceCalculator), ix);
				routing.SetFixedCostOfVehicle((allDrivers[ix].PriorityAtDay - 1) * DriverPriorityPenalty, ix);
				//Устанавливаем время окончания рабочего дня у водителя.
				if(allDrivers[ix].EndOfDay.HasValue)
					routing.CumulVar(routing.End(ix), "Time").SetMax((long)allDrivers[ix].EndOfDay.Value.TotalSeconds);
			}

			for(int ix = 0; ix < Nodes.Length; ix++)
			{
				var startWindow = Nodes[ix].Order.DeliverySchedule.From.TotalSeconds;
				var endWindow = Nodes[ix].Order.DeliverySchedule.To.TotalSeconds - Nodes[ix].Order.CalculateTimeOnPoint(false) * 60; //FIXME Внимание здесь задаем экспедитора. Это не равильно, при реализации работы с экспедитором нужно это изменить.
				if(endWindow < startWindow)
				{
					logger.Warn("Время разгрузки на точке, не помещается в диапазон времени доставки. {0}-{1}", Nodes[ix].Order.DeliverySchedule.From, Nodes[ix].Order.DeliverySchedule.To);
					endWindow = startWindow;
				}
				time_dimension.CumulVar(ix + 1).SetRange((long)startWindow, (long)endWindow);
				routing.AddDisjunction(new int[]{ix}, MaxDistanceAddressPenalty);
			}

			RoutingSearchParameters search_parameters =
			        RoutingModel.DefaultSearchParameters();
			// Setting first solution heuristic (cheapest addition).
			search_parameters.FirstSolutionStrategy =
				                 FirstSolutionStrategy.Types.Value.ParallelCheapestInsertion;
			
			search_parameters.TimeLimitMs = MaxTimeSeconds * 1000;
			search_parameters.FingerprintArcCostEvaluators = true;
			//search_parameters.OptimizationStep = 100;

			var solver = routing.solver();

			PerformanceHelper.AddTimePoint(logger, $"Настроили оптимизацию");
			MainClass.MainWin.ProgressAdd();
			logger.Info("Закрываем модель...");
			logger.Info("Рассчет расстояний между точками...");
			routing.CloseModelWithParameters(search_parameters);
			#if DEBUG
			PrintMatrixCount(distanceCalculator.matrixcount);
			#endif
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
			#if DEBUG
			PrintMatrixCount(distanceCalculator.matrixcount);
			#endif
			Console.WriteLine("Status = {0}", routing.Status());
			if(solution != null) {
				// Solution cost.
				Console.WriteLine("Cost = {0}", solution.ObjectiveValue());
				ProposedRoutes.Clear();
				time_dimension = routing.GetDimensionOrDie("Time");

				for(int route_number = 0; route_number < routing.Vehicles(); route_number++)
				{
					//FIXME Нужно понять, есть ли у водителя маршрут.
					var route = new ProposedRoute(allDrivers[route_number]);
					long first_node = routing.Start(route_number);
					long second_node = solution.Value(routing.NextVar(first_node)); // Пропускаем первый узел, так как это наша база.
					route.RouteCost = routing.GetCost(first_node, second_node, route_number);

					while(!routing.IsEnd(second_node))
					{
						var time_var = time_dimension.CumulVar(second_node);
						var rPoint = new ProposedRoutePoint(
							TimeSpan.FromSeconds(solution.Min(time_var)),
							TimeSpan.FromSeconds(solution.Max(time_var)),
							Nodes[second_node - 1].Order
						);
						rPoint.DebugMaxMin = String.Format("\n({0},{1})[{3}-{4}]-{2}",
						                                   new DateTime().AddSeconds(solution.Min(time_var)).ToShortTimeString(),
						                                   new DateTime().AddSeconds(solution.Max(time_var)).ToShortTimeString(),
						                                   second_node,
						                                   rPoint.Order.DeliverySchedule.From.ToString("hh\\:mm"),
						                                   rPoint.Order.DeliverySchedule.To.ToString("hh\\:mm")
														  );
						route.Orders.Add(rPoint);
						
						first_node = second_node;
						second_node = solution.Value(routing.NextVar(first_node));
						route.RouteCost += routing.GetCost(first_node, second_node, route_number);
					}

					if(route.Orders.Count > 0)
					{
						ProposedRoutes.Add(route);
						logger.Debug("Маршрут {0}: {1}",
						             route.Driver.Employee.ShortName,
						             String.Join(" -> ", route.Orders.Select(x => x.DebugMaxMin))
						            );
					}
				}
			}

			MainClass.MainWin.ProgressAdd();

			if(ProposedRoutes.Count > 0)
				logger.Info($"Предложено {ProposedRoutes.Count} маршрутов.");
			PerformanceHelper.Main.PrintAllPoints(logger);

			if(distanceCalculator.ErrorWays.Count > 0)
			{
				logger.Debug("Ошибок получения расстояний {0}", distanceCalculator.ErrorWays.Count);
				var uniqueFrom = distanceCalculator.ErrorWays.Select(x => x.FromHash).Distinct().ToList();
				var uniqueTo = distanceCalculator.ErrorWays.Select(x => x.ToHash).Distinct().ToList();
				logger.Debug("Уникальных точек: отправки = {0}, прибытия = {1}", uniqueFrom.Count, uniqueTo.Count);
				logger.Debug("Проблемные точки отправки:\n{0}",
				             String.Join("; ", distanceCalculator.ErrorWays
				                         .GroupBy(x => x.FromHash)
				                         .Where(x => x.Count() > (uniqueTo.Count / 2))
				                         .Select(x => CachedDistance.GetText(x.Key)))
				            );
				logger.Debug("Проблемные точки прибытия:\n{0}",
			 			String.Join("; ", distanceCalculator.ErrorWays
				                    .GroupBy(x => x.ToHash)
				                    .Where(x => x.Count() > (uniqueFrom.Count / 2))
						 			.Select(x => CachedDistance.GetText(x.Key)))
			);

			}
		}

		private void PrintMatrixCount(int[,] matrix)
		{
			StringBuilder matrixText = new StringBuilder(" ");
			for(int x = 0; x < matrix.GetLength(1); x++)
				matrixText.Append(x % 10);

			for(int y = 0; y < matrix.GetLength(0); y++) {
				matrixText.Append("\n" + y % 10);
				for(int x = 0; x < matrix.GetLength(1); x++)
					matrixText.Append(matrix[y, x] > 9 ? "+" : matrix[y, x].ToString());
			}
			logger.Debug(matrixText);
		}
	}
}
