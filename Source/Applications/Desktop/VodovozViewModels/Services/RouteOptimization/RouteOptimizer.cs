using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Google.OrTools.ConstraintSolver;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Osrm;
using QS.Utilities.Debug;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Settings.Common;

namespace Vodovoz.ViewModels.Services.RouteOptimization
{
	// <summary>
	// Главный класс построение оптимальных маршрутов.
	// </summary>
	// <remarks>
	// <para>
	// Имеет 2 основных метода, расчет всего дня <c>CreateRoutes()</c> и 
	// перестройка одного маршрута <c>RebuidOneRoute()</c>.
	// </para>
	// <para>
	// Класс так же содержит статические поля всевозможных коэфициентов применяемых при оптимизации маршрута.
	// Все настроечные коэфециенты находятся в регионе "Настройки оптимизации"
	// </para>
	// </remarks>
	public class RouteOptimizer : PropertyChangedBase, IRouteOptimizer
	{
		private readonly ILogger<RouteOptimizer> _logger;
		private readonly ILogger<CallbackTime> _callbackTimelogger;
		private readonly ILogger<CallbackMonitor> _callbackMonitorLogger;
		private readonly ILogger<CallbackBottles> _callbackBottlesLogger;
		private readonly ILogger<CallbackWeight> _callbackWeightlogger;
		private readonly ILogger<CallbackVolume> _callbackVolumelogger;
		private readonly ILogger<CallbackDistance> _callbackDistanceLogger;
		private readonly ILogger<CallbackDistanceDistrict> _callbackDistanceDistrictlogger;
		private readonly ILogger<ExtDistanceCalculator> _extDistanceCalculatorLogger;
		private readonly IOsrmSettings _osrmSettings;
		private readonly IOsrmClient _osrmClient;
		private readonly ICachedDistanceRepository _cachedDistanceRepository;
		private readonly IGeographicGroupRepository _geographicGroupRepository;

		#region Настройки оптимизации

		// <summary>
		// Штраф за поездку в отсутствующий в списке водителя район.
		// </summary>
		public static long UnlikeDistrictPenalty { get; } = 500000;//100000;

		// <summary>
		// Штраф за передачу заказа другому водителю, если заказ уже находится в маршрутном листе сформированным до начала оптимизации.
		// </summary>
		public static long RemoveOrderFromExistRLPenalty { get; } = 100000;

		// <summary>
		// Штраф за каждый шаг приоритета к каждому адресу, в менее приоритеном районе.
		// </summary>
		public static long DistrictPriorityPenalty { get; } = 5000;

		// <summary>
		// Штраф каждому менее приоритетному водителю, за единицу приоритета, при выходе на маршрут.
		// </summary>
		public static long DriverPriorityPenalty { get; } = 20000;

		// <summary>
		// Штраф каждому менее приоритетному водителю на единицу приоритета, на каждом адресе.
		// </summary>
		public static long DriverPriorityAddressPenalty { get; } = 800;

		// <summary>
		// Штраф за неотвезенный заказ. Или максимальное расстояние на которое имеет смысл ехать.
		// </summary>
		public static long MaxDistanceAddressPenalty { get; } = 300000;

		// <summary>
		// Максимальное количество бутелей в заказе для ларгусов.
		// </summary>
		public static int MaxBottlesInOrderForLargus { get; } = 4;

		// <summary>
		// Штраф за добавление в лагрус большего количества бутелей. Сейчас установлено больше чем стоимость недоставки заказа.
		// То есть такого проиходить не может.
		// </summary>
		public static long LargusMaxBottlePenalty { get; } = 500000;

		// <summary>
		// Штраф обычному водителю если он взял себе адрес ларгуса.
		// </summary>
		public static long SmallOrderNotLargusPenalty { get; } = 25000;

		// <summary>
		// Штраф за каждый адрес в маршруте меньше минимального позволенного в настройках машины <see cref="Employee.MinRouteAddresses"/>.
		// </summary>
		public static long MinAddressesInRoutePenalty { get; } = 50000;

		// <summary>
		// Штраф за каждую бутыль в маршруте меньше минимального позволенного в настройках машины <see cref="Employee.MinRouteAddresses"/>.
		// </summary>
		public static long MinBottlesInRoutePenalty { get; } = 10000;

		// <summary>
		// Штраф за адрес из других частей города <see cref="RouteList.GeographicGroups"/>.
		// </summary>
		public static long AddressFromForeignGeographicGroupPenalty { get; } = 500000;

		#endregion Настройки оптимизации

		public IList<RouteList> Routes { get; set; }
		public IList<Order> Orders { get; set; }
		public IList<AtWorkDriver> Drivers { get; set; }
		public IList<AtWorkForwarder> Forwarders { get; set; }

		private CalculatedOrder[] _nodes;
		private IExtDistanceCalculator _distanceCalculator;

		public Action<string> StatisticsTxtAction { get; set; }
		public List<string> WarningMessages { get; } = new List<string>();

		// <summary>
		// Максимальное время работы механизма оптимизации после вызова <c>Solve()</c>. Это время именно оптимизации, время создания модели при этом не учитывается.
		// </summary>
		public int MaxTimeSeconds { get; set; } = 30;

		public IUnitOfWork UoW { get; set; }

		#region Результат
		public List<ProposedRoute> ProposedRoutes { get; } = new List<ProposedRoute>();
		#endregion

		public RouteOptimizer(
			ILogger<RouteOptimizer> logger,
			ILogger<CallbackTime> callbackTimelogger,
			ILogger<CallbackMonitor> callbackMonitorLogger,
			ILogger<CallbackBottles> callbackBottlesLogger,
			ILogger<CallbackWeight> callbackWeightlogger,
			ILogger<CallbackVolume> callbackVolumelogger,
			ILogger<CallbackDistance> callbackDistanceLogger,
			ILogger<CallbackDistanceDistrict> callbackDistanceDistrictlogger,
			ILogger<ExtDistanceCalculator> extDistanceCalculatorLogger,
			IOsrmSettings osrmSettings,
			IOsrmClient osrmClient,
			ICachedDistanceRepository cachedDistanceRepository,
			IGeographicGroupRepository geographicGroupRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_callbackTimelogger = callbackTimelogger ?? throw new ArgumentNullException(nameof(callbackTimelogger));
			_callbackMonitorLogger = callbackMonitorLogger ?? throw new ArgumentNullException(nameof(callbackMonitorLogger));
			_callbackBottlesLogger = callbackBottlesLogger ?? throw new ArgumentNullException(nameof(callbackBottlesLogger));
			_callbackWeightlogger = callbackWeightlogger ?? throw new ArgumentNullException(nameof(callbackWeightlogger));
			_callbackVolumelogger = callbackVolumelogger ?? throw new ArgumentNullException(nameof(callbackVolumelogger));
			_callbackDistanceLogger = callbackDistanceLogger ?? throw new ArgumentNullException(nameof(callbackDistanceLogger));
			_callbackDistanceDistrictlogger = callbackDistanceDistrictlogger ?? throw new ArgumentNullException(nameof(callbackDistanceDistrictlogger));
			_extDistanceCalculatorLogger = extDistanceCalculatorLogger ?? throw new ArgumentNullException(nameof(extDistanceCalculatorLogger));
			_osrmSettings = osrmSettings ?? throw new ArgumentNullException(nameof(osrmSettings));
			_osrmClient = osrmClient ?? throw new ArgumentNullException(nameof(osrmClient));
			_cachedDistanceRepository = cachedDistanceRepository ?? throw new ArgumentNullException(nameof(cachedDistanceRepository));
			_geographicGroupRepository = geographicGroupRepository ?? throw new ArgumentNullException(nameof(geographicGroupRepository));
		}

		// <summary>
		// Метод создаем маршруты на день основываясь на данных всесенных в поля <c>Routes</c>, <c>Orders</c>,
		// <c>Drivers</c> и <c>Forwarders</c>.
		// </summary>
		public void CreateRoutes(DateTime date, TimeSpan drvStartTime, TimeSpan drvEndTime, Func<string, bool> askIfAvailableFunc)
		{
			WarningMessages.Clear();
			ProposedRoutes.Clear(); //Очищаем сразу, так как можем выйти из метода ранее.

			_logger.LogInformation("Подготавливаем заказы...");
			PerformanceHelper.StartMeasurement($"Строим оптимальные маршруты");

			var possibleRoutes = CreatePossibleTrips(drvStartTime, drvEndTime);

			if(!possibleRoutes.Any())
			{
				AddWarning("Для построения маршрутов, нет водителей.");
				return;
			}

			TestCars(possibleRoutes);

			_nodes = CalculateOrders(possibleRoutes);

			// Создаем калькулятор расчета расстояний. Он сразу запрашивает уже имеющиеся расстояния из кеша
			// и в фоновом режиме начинает считать недостающую матрицу.

			var geoGroupVersions = _geographicGroupRepository.GetGeographicGroupVersionsOnDate(UoW, date);
			_distanceCalculator = new ExtDistanceCalculator(_extDistanceCalculatorLogger, _osrmSettings, _osrmClient, _cachedDistanceRepository, _nodes.Select(x => x.Order.DeliveryPoint).ToArray(), geoGroupVersions, StatisticsTxtAction);

			_logger.LogInformation("Развозка по {DistrictCount} районам.", _nodes.Select(x => x.District).Distinct().Count());

			PerformanceHelper.AddTimePoint($"Подготовка заказов");

			// Пред запуском оптимизации мы должны создать модель и внести в нее все необходимые данные.
			_logger.LogInformation("Создаем модель...");

			RoutingModel routing = new RoutingModel(_nodes.Length + 1, possibleRoutes.Length, 0);

			// Создаем измерение со временем на маршруте.
			// <c>horizon</c> - ограничивает максимально допустимое значение диапазона, чтобы не уйти за границы суток;
			// <c>maxWaitTime</c> - Максимальное время ожидания водителя. То есть водитель закончил разгрузку следующий
			// адрес в маршруте у него не должен быть позже чем на 4 часа ожидания.
			int horizon = 24 * 3600;
			int maxWaitTime = 4 * 3600;
			var timeEvaluators = possibleRoutes.Select(x => new CallbackTime(_callbackTimelogger, _nodes, x, _distanceCalculator)).ToArray();
			routing.AddDimensionWithVehicleTransits(timeEvaluators, maxWaitTime, horizon, false, "Time");
			var timeDimension = routing.GetDimensionOrDie("Time");

			// Ниже заполняем все измерения для учета бутылей, веса, адресов, объема.
			var bottlesCapacity = possibleRoutes.Select(x => (long)x.Car.MaxBottles).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackBottles(_callbackBottlesLogger, _nodes), 0, bottlesCapacity, true, "Bottles");

			var weightCapacity = possibleRoutes.Select(x => (long)x.Car.CarModel.MaxWeight).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackWeight(_callbackWeightlogger, _nodes), 0, weightCapacity, true, "Weight");

			var volumeCapacity = possibleRoutes.Select(x => (long)(x.Car.CarModel.MaxVolume * 1000)).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackVolume(_callbackVolumelogger, _nodes), 0, volumeCapacity, true, "Volume");

			var addressCapacity = possibleRoutes.Select(x => (long)x.Driver.MaxRouteAddresses).ToArray();
			routing.AddDimensionWithVehicleCapacity(new CallbackAddressCount(_nodes.Length), 0, addressCapacity, true, "AddressCount");

			var bottlesDimension = routing.GetDimensionOrDie("Bottles");
			var addressDimension = routing.GetDimensionOrDie("AddressCount");

			for(int ix = 0; ix < possibleRoutes.Length; ix++)
			{
				// Устанавливаем функцию получения стоимости маршрута.
				routing.SetArcCostEvaluatorOfVehicle(new CallbackDistanceDistrict(_callbackDistanceDistrictlogger, _nodes, possibleRoutes[ix], _distanceCalculator), ix);

				// Добавляем фиксированный штраф за принадлежность водителя.
				if(possibleRoutes[ix].Driver.DriverType.HasValue)
				{
					routing.SetFixedCostOfVehicle((int)possibleRoutes[ix].Driver.DriverType * DriverPriorityPenalty, ix);
				}
				else
				{
					routing.SetFixedCostOfVehicle(DriverPriorityPenalty * 3, ix);
				}

				var cumulTimeOnEnd = routing.CumulVar(routing.End(ix), "Time");
				var cumulTimeOnBegin = routing.CumulVar(routing.Start(ix), "Time");

				// Устанавливаем минимальные(мягкие) границы для измерений. При значениях меньше минимальных, маршрут все таки принимается,
				// но вносятся некоторые штрафные очки на последнюю точку маршрута.
				//bottlesDimension.SetEndCumulVarSoftLowerBound(ix, possibleRoutes[ix].Car.MinBottles, MinBottlesInRoutePenalty);
				//addressDimension.SetEndCumulVarSoftLowerBound(ix, possibleRoutes[ix].Driver.MinRouteAddresses, MinAddressesInRoutePenalty);

				// Устанавливаем диапазон времени для движения по маршруту в зависимости от выбраной смены,
				// день, вечер и с учетом досрочного завершения водителем работы.
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
				{
					cumulTimeOnEnd.SetMax((long)possibleRoutes[ix].EarlyEnd.Value.TotalSeconds);
				}
			}

			for(int ix = 0; ix < _nodes.Length; ix++)
			{
				// Проставляем на каждый адрес окно времени приезда.
				var startWindow = _nodes[ix].Order.DeliverySchedule.From.TotalSeconds;
				var endWindow = _nodes[ix].Order.DeliverySchedule.To.TotalSeconds - _nodes[ix].Order.CalculateTimeOnPoint(false); //FIXME Внимание здесь задаем время без экспедитора и без учета скорости водителя. Это не правильно, но другого варианта я придумать не смог.
				if(endWindow < startWindow)
				{
					AddWarning(
						"Время разгрузки на {DeliveryPointShortAddress}, не помещается в диапазон времени доставки. {DeliveryScheduleFrom}-{DeliveryScheduleTo}",
						_nodes[ix].Order.DeliveryPoint.ShortAddress,
						_nodes[ix].Order.DeliverySchedule.From,
						_nodes[ix].Order.DeliverySchedule.To);

					endWindow = startWindow;
				}

				timeDimension.CumulVar(ix + 1).SetRange((long)startWindow, (long)endWindow);

				// Добавляем абсолютно все заказы в дизюкцию. Если бы заказы небыли вдобавлены в отдельные дизьюкции
				// то при не возможность доставить хоть один заказ. Все решение бы считаль не верным. Добавление каждого заказа
				// в отдельную дизьюкцию, позволяет механизму не вести какой то и заказов, и все таки формировать решение с недовезенными
				// заказами. Дизьюкция работает так. Он говорит, если хотя бы один заказ в этой группе(дизьюкции) доставлен,
				// то все хорошо, иначе штраф. Так как у нас в кадой дизьюкции по одному заказу. Мы получаем опциональную доставку каждого заказа.
				routing.AddDisjunction(new int[] { ix + 1 }, MaxDistanceAddressPenalty);
			}

			_logger.LogDebug("Nodes.Length = {NodesLength}", _nodes.Length);
			_logger.LogDebug("routing.Nodes() = {routingNodes}", routing.Nodes());
			_logger.LogDebug("GetNumberOfDisjunctions = {GetNumberOfDisjunctions}", routing.GetNumberOfDisjunctions());

			RoutingSearchParameters searchParameters = RoutingModel.DefaultSearchParameters();
			// Setting first solution heuristic (cheapest addition).
			// Указывается стратегия первоначального заполнения. Опытным путем было вычислено, что именно при 
			// стратегиях вставки маршруты получаются с набором точек более близких к друг другу. То есть в большей
			// степени облачком. Что воспринималось человеком как более отпимальное. В отличии от большенства других
			// стратегий в которых маршруты, формируюся скорее по лентами ведущими через все обезжаемые раоны. То есть водители
			// чаще имели пересечения маршутов.
			searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.ParallelCheapestInsertion;

			searchParameters.TimeLimitMs = MaxTimeSeconds * 1000;
			// Отключаем внутреннего кеширования расчитанных значений. Опытным путем было проверено, что включение этого значения.
			// Значительно(на несколько секунд) увеличивает время закрытия модели и сокращает иногда не значительно время расчета оптимизаций.
			// И в принцепе становится целесообразно только на количествах заказов 300-400. При количестве заказов менее 200
			// влючение отпечатков значений. Не уменьшало, а увеличивало общее время расчета. А при большом количестве заказов
			// время расчета уменьшалось не значительно.
			//searchParameters.FingerprintArcCostEvaluators = false;

			searchParameters.FingerprintArcCostEvaluators = true;

			//searchParameters.OptimizationStep = 100;

			var solver = routing.solver();

			PerformanceHelper.AddTimePoint($"Настроили оптимизацию");
			_logger.LogInformation("Закрываем модель...");

			if(WarningMessages.Any() && !askIfAvailableFunc.Invoke(
				string.Format(
					"При построении транспортной модели обнаружены следующие проблемы:\n{0}\nПродолжить?",
					string.Join("\n", WarningMessages.Select(x => "⚠ " + x)))))
			{
				_distanceCalculator.Canceled = true;
				_distanceCalculator.Dispose();
				return;
			}

			_logger.LogInformation("Рассчет расстояний между точками...");
			routing.CloseModelWithParameters(searchParameters);
#if DEBUG
			PrintMatrixCount(_distanceCalculator.MatrixCount);
#endif
			//Записывем возможно не схраненый кеш в базу.
			_distanceCalculator.FlushCache();
			//Попытка хоть как то ослеживать что происходит в момент построения. Возможно не очень правильная.
			//Пришлось создавать 2 монитора.
			var lastSolution = solver.MakeLastSolutionCollector();
			lastSolution.AddObjective(routing.CostVar());
			routing.AddSearchMonitor(lastSolution);
			routing.AddSearchMonitor(new CallbackMonitor(_callbackMonitorLogger, solver, StatisticsTxtAction, lastSolution));

			PerformanceHelper.AddTimePoint($"Закрыли модель");
			_logger.LogInformation("Поиск решения...");

			Assignment solution = routing.SolveWithParameters(searchParameters);
			PerformanceHelper.AddTimePoint($"Получили решение.");
			_logger.LogInformation("Готово. Заполняем.");
#if DEBUG
			PrintMatrixCount(_distanceCalculator.MatrixCount);
#endif
			_logger.LogInformation("Status = {RoutingStatus}", routing.Status());
			if(solution != null)
			{
				// Solution cost.
				_logger.LogInformation("Cost = {Cost}", solution.ObjectiveValue());
				timeDimension = routing.GetDimensionOrDie("Time");

				//Читаем полученные маршруты.
				for(int routeNumber = 0; routeNumber < routing.Vehicles(); routeNumber++)
				{
					var route = new ProposedRoute(possibleRoutes[routeNumber]);
					long firstNode = routing.Start(routeNumber);
					long secondNode = solution.Value(routing.NextVar(firstNode)); // Пропускаем первый узел, так как это наша база.
					route.RouteCost = routing.GetCost(firstNode, secondNode, routeNumber);

					while(!routing.IsEnd(secondNode))
					{
						IntVar timeVar = timeDimension.CumulVar(secondNode);
						var proposedRoutePoint = new ProposedRoutePoint(
							TimeSpan.FromSeconds(solution.Min(timeVar)),
							TimeSpan.FromSeconds(solution.Max(timeVar)),
							_nodes[secondNode - 1].Order
						);

						proposedRoutePoint.DebugMaxMin = $"\n({new DateTime().AddSeconds(solution.Min(timeVar)).ToShortTimeString()},{new DateTime().AddSeconds(solution.Max(timeVar)).ToShortTimeString()})[{proposedRoutePoint.Order.DeliverySchedule.From:hh\\:mm}-{proposedRoutePoint.Order.DeliverySchedule.To:hh\\:mm}]-{secondNode} Cost:{routing.GetCost(firstNode, secondNode, routeNumber)}";

						route.Orders.Add(proposedRoutePoint);

						firstNode = secondNode;
						secondNode = solution.Value(routing.NextVar(firstNode));
						route.RouteCost += routing.GetCost(firstNode, secondNode, routeNumber);
					}

					if(route.Orders.Count > 0)
					{
						ProposedRoutes.Add(route);
						_logger.LogDebug("Маршрут {DriverName}: {OrdersMaxMin}",
							route.Trip.Driver.ShortName,
							string.Join(" -> ", route.Orders.Select(x => x.DebugMaxMin)));
					}
					else
					{
						_logger.LogDebug("Маршрут {DriverName}: пустой", route.Trip.Driver.ShortName);
					}
				}
			}

#if DEBUG
			_logger.LogDebug("SGoToBase:{0}", string.Join(", ", CallbackDistanceDistrict.SGoToBase.Select(x => $"{x.Key.Driver.ShortName}={x.Value}")));
			_logger.LogDebug("SFromExistPenality:{0}", string.Join(", ", CallbackDistanceDistrict.SFromExistPenality.Select(x => $"{x.Key.Driver.ShortName}={x.Value}")));
			_logger.LogDebug("SUnlikeDistrictPenality:{0}", string.Join(", ", CallbackDistanceDistrict.SUnlikeDistrictPenality.Select(x => $"{x.Key.Driver.ShortName}={x.Value}")));
			_logger.LogDebug("SLargusPenality:{0}", string.Join(", ", CallbackDistanceDistrict.SLargusPenality.Select(x => $"{x.Key.Driver.ShortName}={x.Value}")));
#endif

			if(ProposedRoutes.Count > 0)
			{
				_logger.LogInformation("Предложено {ProposedRoutesCount} маршрутов.", ProposedRoutes.Count);
			}

			PerformanceHelper.Main.PrintAllPoints(_logger);

			if(_distanceCalculator.ErrorWays.Any())
			{
				_logger.LogDebug("Ошибок получения расстояний {ErrorWaysCount}", _distanceCalculator.ErrorWays.Count);
				var uniqueFrom = _distanceCalculator.ErrorWays.Select(x => x.FromHash).Distinct().ToList();
				var uniqueTo = _distanceCalculator.ErrorWays.Select(x => x.ToHash).Distinct().ToList();

				_logger.LogDebug("Уникальных точек: отправки = {UniqueFromCount}, прибытия = {UniqueToCount}", uniqueFrom.Count, uniqueTo.Count);
				_logger.LogDebug("Проблемные точки отправки:\n{ErrorWaysFrom}",
					string.Join("; ", _distanceCalculator.ErrorWays
						.GroupBy(x => x.FromHash)
						.Where(x => x.Count() > uniqueTo.Count / 2)
						.Select(x => CachedDistance.GetText(x.Key))));

				_logger.LogDebug("Проблемные точки прибытия:\n{ErrorWaysTo}",
			 		string.Join("; ", _distanceCalculator.ErrorWays
						.GroupBy(x => x.ToHash)
						.Where(x => x.Count() > uniqueFrom.Count / 2)
						.Select(x => CachedDistance.GetText(x.Key))));
			}
		}

		private CalculatedOrder[] CalculateOrders(PossibleTrip[] possibleRoutes)
		{
			var areas = UoW.GetAll<District>()
							.Where(x => x.DistrictsSet.Status == DistrictsSetStatus.Active)
							.ToList();

			var unusedDistricts = new List<District>();
			var calculatedOrders = new List<CalculatedOrder>();

			// Перебираем все заказы, исключаем те которые без координат, определяем для каждого заказа район
			// на основании координат. И создавая экземпляр <c>CalculatedOrder</c>, происходит подсчет сумарной
			// информации о заказе. Всего бутылей, вес и прочее.
			foreach(var order in Orders)
			{
				if(order.DeliveryPoint.Longitude == null || order.DeliveryPoint.Latitude == null)
				{
					continue;
				}

				var point = new Point((double)order.DeliveryPoint.Latitude.Value, (double)order.DeliveryPoint.Longitude.Value);
				var area = areas.Find(x => x.DistrictBorder.Contains(point));

				if(area != null)
				{
					var oldRoute = Routes.FirstOrDefault(r => r.Addresses.Any(a => a.Order.Id == order.Id));

					if(oldRoute != null)
					{
						calculatedOrders.Add(new CalculatedOrder(order, area, false, oldRoute));
					}
					else if(possibleRoutes.SelectMany(x => x.Districts).Any(x => x.District.Id == area.Id))
					{
						var cOrder = new CalculatedOrder(order, area);
						//if(possibleRoutes.Any(r => r.GeographicGroup.Id == cOrder.ShippingBase.Id))//убрать, если в автоформировании должны учавствовать заказы из всех частей города вне зависимости от того какие части города выбраны в диалоге
						calculatedOrders.Add(cOrder);
					}
					else if(!unusedDistricts.Contains(area))
					{
						unusedDistricts.Add(area);
					}
				}
			}

			if(unusedDistricts.Any())
			{
				AddWarning("Районы без водителей: {DistrictsNames}", string.Join(", ", unusedDistricts.Select(x => x.DistrictName)));
			}

			return calculatedOrders.ToArray();
		}

		private PossibleTrip[] CreatePossibleTrips(TimeSpan drvStartTime, TimeSpan drvEndTime)
		{
			// Создаем список поездок всех водителей. Тут перебираем всех водителей с машинами
			// и создаем поездки для них, в зависимости от выбранного режима работы.
			var trips = Drivers
				.Where(x => x.Car != null)
				.OrderBy(x => x.PriorityAtDay)
				.SelectMany(drv => drv.DaySchedule != null
					? drv.DaySchedule.Shifts
						.Where(s => s.StartTime >= drvStartTime
							&& s.StartTime < drvEndTime)
						.Select(shift => new PossibleTrip(drv, shift))
					: new[] { new PossibleTrip(drv, null) })
				.ToList();

			// Стыкуем уже созданные маршрутные листы с возможными поездками, на основании водителя и смены.
			// Если уже созданный маршрут не найден в поездках, то создаем поездку для него.
			foreach(var existRoute in Routes)
			{
				var trip = trips.FirstOrDefault(x => x.Driver == existRoute.Driver && x.Shift == existRoute.Shift);

				if(trip != null)
				{
					trip.OldRoute = existRoute;
				}
				else
				{
					trips.Add(new PossibleTrip(existRoute));
				}

				//Проверяем все ли заказы из МЛ присутствуют в списке заказов. Если их нет. Добавляем.
				foreach(var address in existRoute.Addresses)
				{
					if(Orders.All(x => x.Id != address.Order.Id))
					{
						Orders.Add(address.Order);
					}
				}
			}

			return trips.ToArray();
		}

		// <summary>
		// Получаем предложение по оптимальному расположению адресов в указанном маршруте.
		// Рачет идет с учетом окон доставки. Но естественно без любых ограничений по весу и прочему.
		// </summary>
		// <returns>Предолженый маршрут</returns>
		// <param name="route">Первоначальный маршрутный лист, чтобы взять адреса.</param>
		public ProposedRoute RebuidOneRoute(RouteList route)
		{
			var trip = new PossibleTrip(route);

			_logger.LogInformation("Подготавливаем заказы...");
			PerformanceHelper.StartMeasurement($"Строим маршрут");

			var calculatedOrders = new List<CalculatedOrder>();

			foreach(var address in route.Addresses)
			{
				if(address.Order.DeliveryPoint.Longitude == null || address.Order.DeliveryPoint.Latitude == null)
				{
					continue;
				}

				calculatedOrders.Add(new CalculatedOrder(address.Order, null));
			}
			_nodes = calculatedOrders.ToArray();

			var geoGroupVersions = _geographicGroupRepository.GetGeographicGroupVersionsOnDate(route.UoW, route.Date);
			_distanceCalculator = new ExtDistanceCalculator(_extDistanceCalculatorLogger, _osrmSettings, _osrmClient, _cachedDistanceRepository, _nodes.Select(x => x.Order.DeliveryPoint).ToArray(), geoGroupVersions, StatisticsTxtAction);

			PerformanceHelper.AddTimePoint($"Подготовка заказов");

			_logger.LogInformation("Создаем модель...");
			var routing = new RoutingModel(_nodes.Length + 1, 1, 0);

			int horizon = 24 * 3600;

			routing.AddDimension(new CallbackTime(_callbackTimelogger, _nodes, trip, _distanceCalculator), 3 * 3600, horizon, false, "Time");
			var timeDimension = routing.GetDimensionOrDie("Time");

			var cumulTimeOnEnd = routing.CumulVar(routing.End(0), "Time");
			var cumulTimeOnBegin = routing.CumulVar(routing.Start(0), "Time");

			if(route.Shift != null)
			{
				var shift = route.Shift;
				cumulTimeOnEnd.SetMax((long)shift.EndTime.TotalSeconds);
				cumulTimeOnBegin.SetMin((long)shift.StartTime.TotalSeconds);
			}

			routing.SetArcCostEvaluatorOfVehicle(new CallbackDistance(_callbackDistanceLogger, _nodes, _distanceCalculator), 0);

			for(int ix = 0; ix < _nodes.Length; ix++)
			{
				var startWindow = _nodes[ix].Order.DeliverySchedule.From.TotalSeconds;
				var endWindow = _nodes[ix].Order.DeliverySchedule.To.TotalSeconds - trip.Driver.TimeCorrection(_nodes[ix].Order.CalculateTimeOnPoint(route.Forwarder != null));

				if(endWindow < startWindow)
				{
					_logger.LogWarning("Время разгрузки на точке, не помещается в диапазон времени доставки. {DeliveryScheduleFrom}-{DeliveryScheduleTo}", _nodes[ix].Order.DeliverySchedule.From, _nodes[ix].Order.DeliverySchedule.To);
					endWindow = startWindow;
				}

				timeDimension.CumulVar(ix + 1).SetRange((long)startWindow, (long)endWindow);
				routing.AddDisjunction(new int[] { ix + 1 }, MaxDistanceAddressPenalty);
			}

			RoutingSearchParameters searchParameters = RoutingModel.DefaultSearchParameters();
			searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.ParallelCheapestInsertion;

			searchParameters.TimeLimitMs = MaxTimeSeconds * 1000;
			//searchParameters.FingerprintArcCostEvaluators = true;
			//searchParameters.OptimizationStep = 100;

			var solver = routing.solver();

			PerformanceHelper.AddTimePoint($"Настроили оптимизацию");
			_logger.LogInformation("Закрываем модель...");
			_logger.LogInformation("Рассчет расстояний между точками...");
			routing.CloseModelWithParameters(searchParameters);
			_distanceCalculator.FlushCache();
			var lastSolution = solver.MakeLastSolutionCollector();
			lastSolution.AddObjective(routing.CostVar());
			routing.AddSearchMonitor(lastSolution);
			routing.AddSearchMonitor(new CallbackMonitor(_callbackMonitorLogger, solver, StatisticsTxtAction, lastSolution));

			PerformanceHelper.AddTimePoint($"Закрыли модель");
			_logger.LogInformation("Поиск решения...");

			Assignment solution = routing.SolveWithParameters(searchParameters);
			PerformanceHelper.AddTimePoint($"Получили решение.");
			_logger.LogInformation("Готово. Заполняем.");
			_logger.LogInformation("Status = {Status}", routing.Status());

			ProposedRoute proposedRoute = null;

			if(solution != null)
			{
				// Solution cost.
				_logger.LogInformation("Cost = {Cost}", solution.ObjectiveValue());
				timeDimension = routing.GetDimensionOrDie("Time");

				int routeNumber = 0;

				proposedRoute = new ProposedRoute(null);
				long firstNode = routing.Start(routeNumber);
				long secondNode = solution.Value(routing.NextVar(firstNode)); // Пропускаем первый узел, так как это наша база.
				proposedRoute.RouteCost = routing.GetCost(firstNode, secondNode, routeNumber);

				while(!routing.IsEnd(secondNode))
				{
					var timeVar = timeDimension.CumulVar(secondNode);
					var proposedRoutePoint = new ProposedRoutePoint(
						TimeSpan.FromSeconds(solution.Min(timeVar)),
						TimeSpan.FromSeconds(solution.Max(timeVar)),
						_nodes[secondNode - 1].Order
					);

					proposedRoutePoint.DebugMaxMin = $"\n({new DateTime().AddSeconds(solution.Min(timeVar)).ToShortTimeString()},{new DateTime().AddSeconds(solution.Max(timeVar)).ToShortTimeString()})[{proposedRoutePoint.Order.DeliverySchedule.From:hh\\:mm}-{proposedRoutePoint.Order.DeliverySchedule.To:hh\\:mm}]-{secondNode}";
					proposedRoute.Orders.Add(proposedRoutePoint);

					firstNode = secondNode;
					secondNode = solution.Value(routing.NextVar(firstNode));
					proposedRoute.RouteCost += routing.GetCost(firstNode, secondNode, routeNumber);
				}
			}

			PerformanceHelper.Main.PrintAllPoints(_logger);

			if(_distanceCalculator.ErrorWays.Count > 0)
			{
				_logger.LogDebug("Ошибок получения расстояний {ErrorWaysCount}", _distanceCalculator.ErrorWays.Count);
				var uniqueFrom = _distanceCalculator.ErrorWays.Select(x => x.FromHash).Distinct().ToList();
				var uniqueTo = _distanceCalculator.ErrorWays.Select(x => x.ToHash).Distinct().ToList();

				_logger.LogDebug("Уникальных точек: отправки = {UniqueFromCount}, прибытия = {UniqueToCount}", uniqueFrom.Count, uniqueTo.Count);
				_logger.LogDebug("Проблемные точки отправки:\n{ErrorWaysFrom}",
					string.Join("; ", _distanceCalculator.ErrorWays
						.GroupBy(x => x.FromHash)
						.Where(x => x.Count() > uniqueTo.Count / 2)
						.Select(x => CachedDistance.GetText(x.Key))));

				_logger.LogDebug("Проблемные точки прибытия:\n{ErrorWaysTo}",
			 		string.Join("; ", _distanceCalculator.ErrorWays
						.GroupBy(x => x.ToHash)
						.Where(x => x.Count() > uniqueFrom.Count / 2)
						.Select(x => CachedDistance.GetText(x.Key))));
			}

			_distanceCalculator.Dispose();
			return proposedRoute;
		}

		private void PrintMatrixCount(int[,] matrix)
		{
			var matrixText = new StringBuilder(" ");

			for(int x = 0; x < matrix.GetLength(1); x++)
			{
				matrixText.Append(x % 10);
			}

			for(int y = 0; y < matrix.GetLength(0); y++)
			{
				matrixText.Append("\n" + y % 10);

				for(int x = 0; x < matrix.GetLength(1); x++)
				{
					matrixText.Append(matrix[y, x] > 9 ? "+" : matrix[y, x].ToString());
				}
			}

			_logger.LogDebug(matrixText.ToString());
		}

		private void AddWarning(string text)
		{
			_logger.LogWarning(text);
			WarningMessages.Add(text);
		}

		private void AddWarning(string text, params object[] args)
		{
			_logger.LogWarning(text, args);

			var argReplacement = new List<string>();

			var matches = Regex.Matches(text, "{(.*)}");

			foreach(Match match in matches)
			{
				if(!argReplacement.Contains(match.Value))
				{
					argReplacement.Add(match.Value);
				}
			}

			var matchEveluator = new MatchEvaluator((match) => "{" + argReplacement.IndexOf(match.Value) + "}");

			var result = Regex.Replace(text, "{(.*)}", matchEveluator);

			WarningMessages.Add(string.Format(result, args));
		}

		// <summary>
		// Метод проверят список поездок и создает предупреждающие сообщения о возможных проблемах с настройками машин.
		// </summary>
		private void TestCars(PossibleTrip[] trips)
		{
			var addressProblems = trips.Select(x => x.Driver).Distinct().Where(x => x.MaxRouteAddresses < 1).ToList();

			if(addressProblems.Count > 1)
			{
				AddWarning("Водителям {Drivers} не будут назначены заказы, так как максимальное количество адресов у них меньше 1",
					string.Join(", ", addressProblems.Select(x => x.ShortName)));
			}

			var bottlesProblems = trips.Select(x => x.Car).Distinct().Where(x => x.MaxBottles < 1).ToList();

			if(bottlesProblems.Count > 1)
			{
				AddWarning("Автомобили {Cars} не смогут везти воду, так как максимальное количество бутылей у них меньше 1.",
					string.Join(", ", bottlesProblems.Select(x => x.RegistrationNumber)));
			}

			var volumeProblems = trips.Select(x => x.Car).Distinct().Where(x => x.CarModel.MaxVolume < 1).ToList();

			if(volumeProblems.Count > 1)
			{
				AddWarning("Автомобили {Cars} смогут погрузить только безобъёмные товары, так как максимальный объём погрузки у них меньше 1.",
					string.Join(", ", volumeProblems.Select(x => x.RegistrationNumber)));
			}

			var weightProblems = trips.Select(x => x.Car).Distinct().Where(x => x.CarModel.MaxWeight < 1).ToList();

			if(weightProblems.Count > 1)
			{
				AddWarning("Автомобили {Cars} не смогут везти грузы, так как грузоподъёмность у них меньше 1 кг.",
					string.Join(", ", weightProblems.Select(x => x.RegistrationNumber)));
			}
		}
	}
}
