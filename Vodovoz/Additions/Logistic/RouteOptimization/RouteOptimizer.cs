using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.OrTools.ConstraintSolver;
using Gtk;
using NetTopologySuite.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Logistic;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	/// <summary>
	/// Главный класс построение оптимальных маршрутов.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Имеет 2 основных метода, расчет всего дня <c>CreateRoutes()</c> и 
	/// перестройка одного маршрута <c>RebuidOneRoute()</c>.
	/// </para>
	/// <para>
	/// Класс так же содержит статические поля всевозможных коэфициентов применяемых при оптимизации маршрута.
	/// Все настроечные коэфециенты находятся в регионе "Настройки оптимизации"
	/// </para>
	/// </remarks>
	public class RouteOptimizer : PropertyChangedBase
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Настройки оптимизации
		/// <summary>
		/// Штраф за поездку в отсутствующий в списке водителя район.
		/// </summary>
		public static long UnlikeDistrictPenalty = 100000;
		/// <summary>
		/// Штраф за передачу заказа другому водителю, если заказ уже находится в маршрутном листе сформированным до начала оптимизации.
		/// </summary>
		public static long RemoveOrderFromExistRLPenalty = 100000;
		/// <summary>
		/// Штраф за каждый шаг приоритета к каждому адресу, в менее приоритеном районе.
		/// </summary>
		public static long DistrictPriorityPenalty = 1000;
		/// <summary>
		/// Штраф каждому менее приоритетному водителю, за единицу приоритета, при выходе на маршрут.
		/// </summary>
		public static long DriverPriorityPenalty = 10000;
		/// <summary>
		/// Штраф каждому менее приоритетному водителю на единицу приоритета, на каждом адресе.
		/// </summary>
		public static long DriverPriorityAddressPenalty = 800;
		/// <summary>
		/// Штраф за неотвезенный заказ. Или максимальное расстояние на которое имеет смысл ехать.
		/// </summary>
		public static long MaxDistanceAddressPenalty = 300000;
		/// <summary>
		/// Максимальное количество бутелей в заказе для ларгусов.
		/// </summary>
		public static int MaxBottlesInOrderForLargus = 42;
		/// <summary>
		/// Штраф за добавление в лагрус большего количества бутелей. Сейчас установлено больше чем стоимость недоставки заказа.
		/// То есть такого проиходить не может.
		/// </summary>
		public static long LargusMaxBottlePenalty = 500000;
		/// <summary>
		/// Штраф обычному водителю если он взял себе адрес ларгуса.
		/// </summary>
		public static long SmallOrderNotLargusPenalty = 25000;
		/// <summary>
		/// Штраф за каждый адрес в маршруте меньше минимального позволенного в настройках машины <c>Car.MinRouteAddresses</c>.
		/// </summary>
		public static long MinAddressesInRoutePenalty = 50000;
		/// <summary>
		/// Штраф за каждую бутыль в маршруте меньше минимального позволенного в настройках машины <c>Car.MinRouteAddresses</c>.
		/// </summary>
		public static long MinBottlesInRoutePenalty = 10000;
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

		/// <summary>
		/// Максимальное время работы механизма оптимизации после вызова <c>Solve()</c>. Это время именно оптимизации, время создания модели при этом не учитывается.
		/// </summary>
		public int MaxTimeSeconds { get; set; } = 30;

		public IUnitOfWork UoW;

		#region Результат
		public List<ProposedRoute> ProposedRoutes = new List<ProposedRoute>();
		#endregion

		public RouteOptimizer()
		{
		}

		/// <summary>
		/// Метод создаем маршруты на день основываясь на данных всесенных в поля <c>Routes</c>, <c>Orders</c>,
		/// <c>Drivers</c> и <c>Forwarders</c>.
		/// </summary>
		public void CreateRoutes()
		{
			WarningMessages.Clear();
			ProposedRoutes.Clear(); //Очищаем сразу, так как можем выйти из метода ранее.

			logger.Info("Подготавливаем заказы...");
			PerformanceHelper.StartMeasurement($"Строим оптимальные маршруты");
			MainClass.MainWin.ProgressStart(4);

			/// Создаем список поездок всех водителей. Тут перебираем всех водителей с машинами
			/// и создаем поездки для них, в зависимости от выбранного режима работы.
			var trips = Drivers.Where(x => x.Car != null)
							   .OrderBy(x => x.PriorityAtDay)
									.SelectMany(x => x.DaySchedule != null
												? x.DaySchedule.Shifts.Select(s => new PossibleTrip(x, s))
												: new[] { new PossibleTrip(x, null) }
											   )
							   .ToList();

			/// Стыкуем уже созданные маршрутные листы с возможными поездками, на основании водителя и смены.
			/// Если уже созданный маршрут не найдет в поездках создаем поездку для него.
			foreach(var existRoute in Routes) {
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

			/// Перебираем все заказы, исключаем те которые без координат, определяем для каждого заказа район
			/// на основании координат. И создавая экземпляр <c>CalculatedOrder</c>, происходит подсчет сумарной
			/// информации о заказа. Всего бутылей, вес и прочее.
			foreach(var order in Orders) {
				if(order.DeliveryPoint.Longitude == null || order.DeliveryPoint.Latitude == null)
					continue;
				var point = new Point((double)order.DeliveryPoint.Latitude.Value, (double)order.DeliveryPoint.Longitude.Value);
				var aria = areas.Find(x => x.Geometry.Contains(point));
				if(aria != null) {
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
			if(unusedDistricts.Count > 0) {
				AddWarning("Районы без водителей: {0}", String.Join(", ", unusedDistricts.Select(x => x.Name)));
			}

			/// Создаем калькулятор расчета расстояний. Он сразу запрашивает уже имеющиеся расстояния из кеша
			/// и в фоновом режиме начинает считать недостающую матрицу.
			distanceCalculator = new ExtDistanceCalculator(DistanceProvider.Osrm, Nodes.Select(x => x.Order.DeliveryPoint).ToArray(), DebugBuffer);

			MainClass.MainWin.ProgressAdd();
			logger.Info("Развозка по {0} районам.", calculatedOrders.Select(x => x.District).Distinct().Count());
			PerformanceHelper.AddTimePoint(logger, $"Подготовка заказов");

			/// Пред запуском оптимизации мы должны создать модель и внести в нее все необходимые данные.
			logger.Info("Создаем модель...");
			RoutingModel routing = new RoutingModel(Nodes.Length + 1, possibleRoutes.Length, 0);

			/// Создаем измерение со временем на маршруте. 
			/// <c>horizon</c> - ограничивает максимально допустимое значение диапазона, чтобы не уйти за границы суток;
			/// <c>maxWaitTime</c> - Максимальное время ожидания водителя. То есть водитель закончил разгрузку следующий
			/// адрес в маршруте у него не должен быть позже чем на 3 часа ожидания.
			int horizon = 24 * 3600;
			int maxWaitTime = 3 * 3600;
			var timeEvaluators = possibleRoutes.Select(x => new CallbackTime(Nodes, x, distanceCalculator)).ToArray();
			routing.AddDimensionWithVehicleTransits(timeEvaluators, maxWaitTime, horizon, false, "Time");
			var time_dimension = routing.GetDimensionOrDie("Time");

			/// Ниже заполняем все измерения для учета бутылей, веса, адресов, объема.
			var bottlesCapacity = possibleRoutes.Select(x => (long)x.Car.MaxBottles).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackBottles(Nodes), 0, bottlesCapacity, true, "Bottles");

			var weightCapacity = possibleRoutes.Select(x => (long)x.Car.MaxWeight).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackWeight(Nodes), 0, weightCapacity, true, "Weight");

			var volumeCapacity = possibleRoutes.Select(x => (long)(x.Car.MaxVolume * 1000)).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackVolume(Nodes), 0, volumeCapacity, true, "Volume");

			var addressCapacity = possibleRoutes.Select(x => (long)(x.Car.MaxRouteAddresses)).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackAddressCount(Nodes.Length), 0, addressCapacity, true, "AddressCount");

			var bottlesDimension = routing.GetDimensionOrDie("Bottles");
			var addressDimension = routing.GetDimensionOrDie("AddressCount");

			for(int ix = 0; ix < possibleRoutes.Length; ix++) {
				/// Устанавливаем функцию получения стоимости маршрута.
				routing.SetArcCostEvaluatorOfVehicle(new CallbackDistanceDistrict(Nodes, possibleRoutes[ix], distanceCalculator), ix);
				/// Добавляем фиксированный штраф за приоритет водителя.
				routing.SetFixedCostOfVehicle((possibleRoutes[ix].DriverPriority - 1) * DriverPriorityPenalty, ix);

				var cumulTimeOnEnd = routing.CumulVar(routing.End(ix), "Time");
				var cumulTimeOnBegin = routing.CumulVar(routing.Start(ix), "Time");

				/// Устанавливаем минимальные(мягкие) границы для измерений. При значениях меньше менемальных, маршрут все таки принимается,
				/// но вносятся некоторые штрафные очки на последнюю точку маршрута.
				bottlesDimension.SetEndCumulVarSoftLowerBound(ix, possibleRoutes[ix].Car.MinBottles, MinBottlesInRoutePenalty);
				addressDimension.SetEndCumulVarSoftLowerBound(ix, possibleRoutes[ix].Car.MinRouteAddresses, MinAddressesInRoutePenalty);

				/// Устанавливаем диапазон времени для движения по маршруту в зависимости от выбраной смены,
				/// день, вечер и с учетом досрочного завершения водителем работы.
				if(possibleRoutes[ix].Shift != null) {
					var shift = possibleRoutes[ix].Shift;
					var endTime = possibleRoutes[ix].EarlyEnd.HasValue
													? Math.Min(shift.EndTime.TotalSeconds, possibleRoutes[ix].EarlyEnd.Value.TotalSeconds)
													: shift.EndTime.TotalSeconds;
					cumulTimeOnEnd.SetMax((long)endTime);
					cumulTimeOnBegin.SetMin((long)shift.StartTime.TotalSeconds);
				} else if(possibleRoutes[ix].EarlyEnd.HasValue) //Устанавливаем время окончания рабочего дня у водителя.
					cumulTimeOnEnd.SetMax((long)possibleRoutes[ix].EarlyEnd.Value.TotalSeconds);
			}

			for(int ix = 0; ix < Nodes.Length; ix++) {
				/// Проставляем на каждый адрес окно времени приезда.
				var startWindow = Nodes[ix].Order.DeliverySchedule.From.TotalSeconds;
				var endWindow = Nodes[ix].Order.DeliverySchedule.To.TotalSeconds - Nodes[ix].Order.CalculateTimeOnPoint(false); //FIXME Внимание здесь задаем время без экспедитора и без учета скорости водителя. Это не правильно, но другого варианта я придумать не смог.
				if(endWindow < startWindow) {
					AddWarning("Время разгрузки на {2}, не помещается в диапазон времени доставки. {0}-{1}", Nodes[ix].Order.DeliverySchedule.From, Nodes[ix].Order.DeliverySchedule.To, Nodes[ix].Order.DeliveryPoint.ShortAddress);
					endWindow = startWindow;
				}
				time_dimension.CumulVar(ix + 1).SetRange((long)startWindow, (long)endWindow);
				/// Добавляем абсолютно все заказы в дизюкцию. Если бы заказы небыли вдобавлены в отдельные дизьюкции
				/// то при не возможность доставить хоть один заказ. Все решение бы считаль не верным. Добавление каждого заказа
				/// в отдельную дизьюкцию, позволяет механизму не вести какой то и заказов, и все таки формировать решение с недовезенными
				/// заказами. Дизьюкция работает так. Он говорит, если хотя бы один заказ в этой группе(дизьюкции) доставлен,
				/// то все хорошо, иначе штраф. Так как у нас в кадой дизьюкции по одному заказу. Мы получаем опциональную доставку каждого заказа.
				routing.AddDisjunction(new int[] { ix + 1 }, MaxDistanceAddressPenalty);
			}

			logger.Debug("Nodes.Length = {0}", Nodes.Length);
			logger.Debug("routing.Nodes() = {0}", routing.Nodes());
			logger.Debug("GetNumberOfDisjunctions = {0}", routing.GetNumberOfDisjunctions());

			RoutingSearchParameters search_parameters =
					RoutingModel.DefaultSearchParameters();
			// Setting first solution heuristic (cheapest addition).
			/// Указывается стратегия первоначального заполнения. Опытным путем было вычислено, что именно при 
			/// стратегиях вставки маршруты получаются с набором точек более близких к друг другу. То есть в большей
			/// степени облачком. Что воспринималось человеком как более отпимальное. В отличии от большенства других
			/// стратегий в которых маршруты, формируюся скорее по лентами ведущими через все обезжаемые раоны. То есть водители
			/// чаще имели пересечения маршутов.
			search_parameters.FirstSolutionStrategy =
								 FirstSolutionStrategy.Types.Value.ParallelCheapestInsertion;

			search_parameters.TimeLimitMs = MaxTimeSeconds * 1000;
			/// Отключаем внутреннего кеширования расчитанных значений. Опытным путем было проверено, что включение этого значения.
			/// Значительно(на несколько секунд) увеличивает время закрытия модели и сокращает иногда не значительно время расчета оптимизаций.
			/// И в принцепе становится целесообразно только на количествах заказов 300-400. При количестве заказов менее 200
			/// влючение отпечатков значений. Не уменьшало, а увеличивало общее время расчета. А при большом количестве заказов
			/// время расчета уменьшалось не значительно.
			search_parameters.FingerprintArcCostEvaluators = false;
			//search_parameters.OptimizationStep = 100;

			var solver = routing.solver();

			PerformanceHelper.AddTimePoint(logger, $"Настроили оптимизацию");
			MainClass.MainWin.ProgressAdd();
			logger.Info("Закрываем модель...");

			if(WarningMessages.Count > 0 &&
				!MessageDialogWorks.RunQuestionDialog("При построении транспортной модели обнаружены следующие проблемы:\n{0}\nПродолжить?",
													 String.Join("\n", WarningMessages.Select(x => "⚠ " + x))))
				return;

			logger.Info("Рассчет расстояний между точками...");
			routing.CloseModelWithParameters(search_parameters);

#if DEBUG
			PrintMatrixCount(distanceCalculator.matrixcount);
#endif
			//Записывем возможно не схраненый кеш в базу.
			distanceCalculator.FlushCache();
			//Попытка хоть как то ослеживать что происходит в момент построения. Возможно не очень правильная.
			//Пришлось создавать 2 монитора.
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
				time_dimension = routing.GetDimensionOrDie("Time");

				//Читаем полученные маршруты.
				for(int route_number = 0; route_number < routing.Vehicles(); route_number++) {
					var route = new ProposedRoute(possibleRoutes[route_number]);
					long first_node = routing.Start(route_number);
					long second_node = solution.Value(routing.NextVar(first_node)); // Пропускаем первый узел, так как это наша база.
					route.RouteCost = routing.GetCost(first_node, second_node, route_number);

					while(!routing.IsEnd(second_node)) {
						var time_var = time_dimension.CumulVar(second_node);
						var rPoint = new ProposedRoutePoint(
							TimeSpan.FromSeconds(solution.Min(time_var)),
							TimeSpan.FromSeconds(solution.Max(time_var)),
							Nodes[second_node - 1].Order
						);
						rPoint.DebugMaxMin = String.Format("\n({0},{1})[{3}-{4}]-{2} Cost:{5}",
														   new DateTime().AddSeconds(solution.Min(time_var)).ToShortTimeString(),
														   new DateTime().AddSeconds(solution.Max(time_var)).ToShortTimeString(),
														   second_node,
														   rPoint.Order.DeliverySchedule.From.ToString("hh\\:mm"),
														   rPoint.Order.DeliverySchedule.To.ToString("hh\\:mm"),
														   routing.GetCost(first_node, second_node, route_number)
														  );
						route.Orders.Add(rPoint);

						first_node = second_node;
						second_node = solution.Value(routing.NextVar(first_node));
						route.RouteCost += routing.GetCost(first_node, second_node, route_number);
					}

					if(route.Orders.Count > 0) {
						ProposedRoutes.Add(route);
						logger.Debug("Маршрут {0}: {1}",
									 route.Trip.Driver.ShortName,
									 String.Join(" -> ", route.Orders.Select(x => x.DebugMaxMin))
									);
					} else
						logger.Debug("Маршрут {0}: пустой", route.Trip.Driver.ShortName);
				}
			}

#if DEBUG
			logger.Debug("SGoToBase:{0}",String.Join(", ", CallbackDistanceDistrict.SGoToBase.Select(x => $"{x.Key.Driver.ShortName}={x.Value}")));
			logger.Debug("SFromExistPenality:{0}", String.Join(", ", CallbackDistanceDistrict.SFromExistPenality.Select(x => $"{x.Key.Driver.ShortName}={x.Value}")));
			logger.Debug("SUnlikeDistrictPenality:{0}", String.Join(", ", CallbackDistanceDistrict.SUnlikeDistrictPenality.Select(x => $"{x.Key.Driver.ShortName}={x.Value}")));
			logger.Debug("SLargusPenality:{0}", String.Join(", ", CallbackDistanceDistrict.SLargusPenality.Select(x => $"{x.Key.Driver.ShortName}={x.Value}")));
#endif

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

		/// <summary>
		/// Получаем предложение по оптимальному расположению адресов в указанном маршруте.
		/// Рачет идет с учетом окон доставки. Но естественно без любых ограничений по весу и прочему.
		/// </summary>
		/// <returns>Предолженый маршрут</returns>
		/// <param name="route">Первоначальный маршрутный лист, чтобы взять адреса.</param>
		public ProposedRoute RebuidOneRoute(RouteList route)
		{
			var trip = new PossibleTrip(route);

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

			routing.AddDimension(new CallbackTime(Nodes, trip, distanceCalculator), 3 * 3600, horizon, false, "Time");
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
				var endWindow = Nodes[ix].Order.DeliverySchedule.To.TotalSeconds - trip.Driver.TimeCorrection(Nodes[ix].Order.CalculateTimeOnPoint(route.Forwarder != null));
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
			//search_parameters.FingerprintArcCostEvaluators = true;
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

		/// <summary>
		/// Метод проверят список поездок и создает предупреждающие сообщения о возможных проблемах с настройками машин.
		/// </summary>
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
