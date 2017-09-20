﻿﻿﻿using System;
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
		public static long RemoveOrderFromExistRLPenalty = 100000; //Штраф за передачу заказа другому водителю, если заказ уже находится в маршрутном листе сформированным до построения.
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
		public List<string> WarningMessages = new List<string>();

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
			WarningMessages.Clear();

			logger.Info("Подготавливаем заказы...");
			PerformanceHelper.StartMeasurement($"Строим оптимальные маршруты");
			MainClass.MainWin.ProgressStart(4);

			//Сортируем в обратном порядке потому что алгоритм отдает предпочтение водителям с конца.
			var trips = Drivers.Where(x => x.Car != null)
			                        .OrderByDescending(x => x.PriorityAtDay)
			                        .SelectMany(x => x.DaySchedule != null 
			                                    ? x.DaySchedule.Shifts.Select(s => new PossibleTrip(x, s)) 
			                                    : new[] { new PossibleTrip(x, null) }
			                                   )
			                   .ToList();

			foreach(var existRoute in Routes)
			{
				var trip = trips.FirstOrDefault(x => x.Driver == existRoute.Driver && x.Shift == existRoute.Shift);
				if(trip != null)
					trip.OldRoute = existRoute;
				else
					trips.Add(new PossibleTrip(existRoute));
				//Проверяем все ли заказы из МЛ присутствуют в списке заказов. Если их нет. Добавляем.
				foreach(var address in existRoute.Addresses) {
					if(!Orders.Any(x => x.Id == address.Order.Id))
						Orders.Add(address.Order);
				}
			}

			var possibleRoutes = trips.ToArray();

			if(possibleRoutes.Length == 0) {
				AddWarning("Для построения маршрутов, нет водителей.");
				return;
			}

			TestCars(possibleRoutes);

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
					var oldRoute = Routes.FirstOrDefault(r => r.Addresses.Any(a => a.Order.Id == order.Id));
					if(oldRoute != null)
						calculatedOrders.Add(new CalculatedOrder(order, aria, false, oldRoute));
					else if(possibleRoutes.SelectMany(x => x.Districts).Any(x => x.District.Id == aria.Id))
						calculatedOrders.Add(new CalculatedOrder(order, aria));
					else if(!unusedDistricts.Contains(aria))
						unusedDistricts.Add(aria);
				}
			}
			Nodes = calculatedOrders.ToArray();
			if(unusedDistricts.Count > 0)
			{
				AddWarning("Районы без водителей: {0}", String.Join(", ", unusedDistricts.Select(x => x.Name)));
			}

			distanceCalculator = new ExtDistanceCalculator(DistanceProvider.Osrm, Nodes.Select(x => x.Order.DeliveryPoint).ToArray(), DebugBuffer);

			MainClass.MainWin.ProgressAdd();
			logger.Info("Развозка по {0} районам.", calculatedOrders.Select(x => x.District).Distinct().Count());
			PerformanceHelper.AddTimePoint(logger, $"Подготовка заказов");

			logger.Info("Создаем модель...");
			RoutingModel routing = new RoutingModel(Nodes.Length + 1, possibleRoutes.Length, 0);

			int horizon = 24 * 3600;

			routing.AddDimension(new CallbackTime(Nodes, null, distanceCalculator), 3 * 3600, horizon, false, "Time");
			var time_dimension = routing.GetDimensionOrDie("Time");

			var bottlesCapacity = possibleRoutes.Select(x => (long)x.Car.MaxBottles).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackBottles(Nodes), 0, bottlesCapacity, true, "Bottles" );

			var weightCapacity = possibleRoutes.Select(x => (long)x.Car.MaxWeight).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackWeight(Nodes), 0, weightCapacity, true, "Weight");

			var volumeCapacity = possibleRoutes.Select(x => (long)(x.Car.MaxVolume * 1000)).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackVolume(Nodes), 0, volumeCapacity, true, "Volume");

			var addressCapacity = possibleRoutes.Select(x => (long)(x.Car.MaxRouteAddresses)).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackAddressCount(Nodes.Length), 0, addressCapacity, true, "AddressCount");

			var bottlesDimension = routing.GetDimensionOrDie("Bottles");

			for(int ix = 0; ix < possibleRoutes.Length; ix++) {
				routing.SetArcCostEvaluatorOfVehicle(new CallbackDistanceDistrict(Nodes, possibleRoutes[ix], distanceCalculator), ix);
				routing.SetFixedCostOfVehicle((possibleRoutes[ix].DriverPriority - 1) * DriverPriorityPenalty, ix);

				var cumulTimeOnEnd = routing.CumulVar(routing.End(ix), "Time");
				var cumulTimeOnBegin = routing.CumulVar(routing.Start(ix), "Time");

				if(possibleRoutes[ix].Shift != null)
				{
					var shift = possibleRoutes[ix].Shift;
					var endTime = possibleRoutes[ix].EarlyEnd.HasValue
					                                ? Math.Min(shift.EndTime.TotalSeconds, possibleRoutes[ix].EarlyEnd.Value.TotalSeconds)
					                                : shift.EndTime.TotalSeconds;
					cumulTimeOnEnd.SetMax((long)endTime);
					cumulTimeOnBegin.SetMin((long)shift.StartTime.TotalSeconds);
				}
				else if(possibleRoutes[ix].EarlyEnd.HasValue) //Устанавливаем время окончания рабочего дня у водителя.
					cumulTimeOnEnd.SetMax((long)possibleRoutes[ix].EarlyEnd.Value.TotalSeconds);
			}

			for(int ix = 0; ix < Nodes.Length; ix++)
			{
				var startWindow = Nodes[ix].Order.DeliverySchedule.From.TotalSeconds;
				var endWindow = Nodes[ix].Order.DeliverySchedule.To.TotalSeconds - Nodes[ix].Order.CalculateTimeOnPoint(false) * 60; //FIXME Внимание здесь задаем экспедитора. Это не равильно, при реализации работы с экспедитором нужно это изменить.
				if(endWindow < startWindow)
				{
					AddWarning("Время разгрузки на {2}, не помещается в диапазон времени доставки. {0}-{1}", Nodes[ix].Order.DeliverySchedule.From, Nodes[ix].Order.DeliverySchedule.To, Nodes[ix].Order.DeliveryPoint.ShortAddress);
					endWindow = startWindow;
				}
				time_dimension.CumulVar(ix + 1).SetRange((long)startWindow, (long)endWindow);
				routing.AddDisjunction(new int[]{ix + 1}, MaxDistanceAddressPenalty);
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
			distanceCalculator.FlushCache();
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
					var route = new ProposedRoute(possibleRoutes[route_number]);
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
						             route.Trip.Driver.ShortName,
						             String.Join(" -> ", route.Orders.Select(x => x.DebugMaxMin))
						            );
					}
					else
						logger.Debug("Маршрут {0}: пустой", route.Trip.Driver.ShortName);
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

		public ProposedRoute RebuidOneRoute(RouteList route)
		{
			logger.Info("Подготавливаем заказы...");
			PerformanceHelper.StartMeasurement($"Строим маршрут");
			MainClass.MainWin.ProgressStart(4);

			List<CalculatedOrder> calculatedOrders = new List<CalculatedOrder>();

			foreach(var address in route.Addresses) {
				if(address.Order.DeliveryPoint.Longitude == null || address.Order.DeliveryPoint.Latitude == null)
					continue;
				
				calculatedOrders.Add(new CalculatedOrder(address.Order, null));
			}
			Nodes = calculatedOrders.ToArray();

			distanceCalculator = new ExtDistanceCalculator(DistanceProvider.Osrm, Nodes.Select(x => x.Order.DeliveryPoint).ToArray(), DebugBuffer);

			MainClass.MainWin.ProgressAdd();
			PerformanceHelper.AddTimePoint(logger, $"Подготовка заказов");

			logger.Info("Создаем модель...");
			RoutingModel routing = new RoutingModel(Nodes.Length + 1, 1, 0);

			int horizon = 24 * 3600;

			routing.AddDimension(new CallbackTime(Nodes, null, distanceCalculator), 3 * 3600, horizon, false, "Time");
			var time_dimension = routing.GetDimensionOrDie("Time");

			var cumulTimeOnEnd = routing.CumulVar(routing.End(0), "Time");
			var cumulTimeOnBegin = routing.CumulVar(routing.Start(0), "Time");

			if(route.Shift != null) {
				var shift = route.Shift;
				cumulTimeOnEnd.SetMax((long)shift.EndTime.TotalSeconds);
				cumulTimeOnBegin.SetMin((long)shift.StartTime.TotalSeconds);
			}

			routing.SetArcCostEvaluatorOfVehicle(new CallbackDistance(Nodes, distanceCalculator), 0);

			for(int ix = 0; ix < Nodes.Length; ix++) {
				var startWindow = Nodes[ix].Order.DeliverySchedule.From.TotalSeconds;
				var endWindow = Nodes[ix].Order.DeliverySchedule.To.TotalSeconds - Nodes[ix].Order.CalculateTimeOnPoint(route.Forwarder != null) * 60;
				if(endWindow < startWindow) {
					logger.Warn("Время разгрузки на точке, не помещается в диапазон времени доставки. {0}-{1}", Nodes[ix].Order.DeliverySchedule.From, Nodes[ix].Order.DeliverySchedule.To);
					endWindow = startWindow;
				}
				time_dimension.CumulVar(ix + 1).SetRange((long)startWindow, (long)endWindow);
				routing.AddDisjunction(new int[] { ix + 1 }, MaxDistanceAddressPenalty);
			}

			RoutingSearchParameters search_parameters =
					RoutingModel.DefaultSearchParameters();
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
			distanceCalculator.FlushCache();
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
			ProposedRoute proposedRoute = null;
			if(solution != null) {
				// Solution cost.
				Console.WriteLine("Cost = {0}", solution.ObjectiveValue());
				time_dimension = routing.GetDimensionOrDie("Time");

				int route_number = 0;
					
				proposedRoute = new ProposedRoute(null);
				long first_node = routing.Start(route_number);
				long second_node = solution.Value(routing.NextVar(first_node)); // Пропускаем первый узел, так как это наша база.
				proposedRoute.RouteCost = routing.GetCost(first_node, second_node, route_number);

				while(!routing.IsEnd(second_node)) {
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
					proposedRoute.Orders.Add(rPoint);

					first_node = second_node;
					second_node = solution.Value(routing.NextVar(first_node));
					proposedRoute.RouteCost += routing.GetCost(first_node, second_node, route_number);
				}
			}

			MainClass.MainWin.ProgressAdd();
			PerformanceHelper.Main.PrintAllPoints(logger);

			if(distanceCalculator.ErrorWays.Count > 0) {
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
			return proposedRoute;
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

		private void AddWarning(string text)
		{
			WarningMessages.Add(text);
			logger.Warn(text);
		}

		private void AddWarning(string text, params object[] args)
		{
			text = String.Format(text, args);
			WarningMessages.Add(text);
			logger.Warn(text);
		}

		private void TestCars(PossibleTrip[] trips)
		{
			var addressProblems = trips.Select(x => x.Car).Distinct().Where(x => x.MaxRouteAddresses < 1).ToList();
			if(addressProblems.Count > 1)
				AddWarning("Автомобилям {0} не будут назначены заказы, так как максимальное количество адресов у них меньше 1.",
				           String.Join(", ", addressProblems.Select(x => x.RegistrationNumber)));

			var bottlesProblems = trips.Select(x => x.Car).Distinct().Where(x => x.MaxBottles < 1).ToList();
			if(bottlesProblems.Count > 1)
				AddWarning("Автомобили {0} не смогут везти воду, так как максимальное количество бутылей у них меньше 1.",
						   String.Join(", ", bottlesProblems.Select(x => x.RegistrationNumber)));

			var volumeProblems = trips.Select(x => x.Car).Distinct().Where(x => x.MaxVolume < 1).ToList();
			if(volumeProblems.Count > 1)
				AddWarning("Автомобили {0} смогут погрузить только безьобемные товары, так как максимальный объем погрузки у них меньше 1.",
						   String.Join(", ", volumeProblems.Select(x => x.RegistrationNumber)));

			var weightProblems = trips.Select(x => x.Car).Distinct().Where(x => x.MaxWeight < 1).ToList();
			if(weightProblems.Count > 1)
				AddWarning("Автомобили {0} не смогут вести грузы, так как грузоподьемность уних меньше 1 кг.",
						   String.Join(", ", weightProblems.Select(x => x.RegistrationNumber)));

		}
	}
}
