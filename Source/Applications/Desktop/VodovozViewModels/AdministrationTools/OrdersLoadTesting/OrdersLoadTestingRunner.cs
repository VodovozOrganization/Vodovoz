using QS.DomainModel.UoW;
using QS.Project.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Settings;
using Employee = Vodovoz.Domain.Employees.Employee;

namespace Vodovoz.ViewModels.AdministrationTools.OrdersLoadTesting
{
	/// <summary>
	/// Нагрузочный тест: в одном потоке цикл
	/// UoW1 — заказ, UoW2 — маршрутный лист на этот заказ (ручное заполнение сущностей).
	/// </summary>
	public class OrdersLoadTestingRunner
	{
		private const string PacsTestDatabaseSettingName = "Pacs.Test.Database";
		private const int DefaultBanknoteForReturn = 5000;
		private const int MaxFixturePoolSize = 200;

		private static readonly PaymentType[] _paymentTypes =
		{
			PaymentType.Cash,
			PaymentType.Terminal,
			PaymentType.Cashless
		};

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDataBaseInfo _dataBaseInfo;
		private readonly ISettingsController _settingsController;

		public OrdersLoadTestingRunner(
			IUnitOfWorkFactory unitOfWorkFactory,
			IDataBaseInfo dataBaseInfo,
			ISettingsController settingsController)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_dataBaseInfo = dataBaseInfo ?? throw new ArgumentNullException(nameof(dataBaseInfo));
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public bool IsTestDatabase()
		{
			if(!_settingsController.ContainsSetting(PacsTestDatabaseSettingName))
			{
				return false;
			}

			var testDatabase = _settingsController.GetStringValue(PacsTestDatabaseSettingName);
			return !string.IsNullOrWhiteSpace(testDatabase)
				&& string.Equals(testDatabase, _dataBaseInfo.Name, StringComparison.OrdinalIgnoreCase);
		}

		public string CurrentDatabaseName => _dataBaseInfo.Name;

		public string ExpectedTestDatabaseName =>
			_settingsController.ContainsSetting(PacsTestDatabaseSettingName)
				? _settingsController.GetStringValue(PacsTestDatabaseSettingName)
				: string.Empty;

		public async Task RunAsync(
			int threadCount,
			Employee author,
			CancellationToken cancellationToken,
			Action<string> log)
		{
			if(threadCount < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(threadCount), "Количество потоков должно быть не меньше 1.");
			}

			if(author == null)
			{
				throw new ArgumentNullException(nameof(author));
			}

			if(!IsTestDatabase())
			{
				throw new InvalidOperationException(
					$"Нагрузочное тестирование разрешено только на тестовой БД.\n" +
					$"Текущая БД: «{CurrentDatabaseName}».\n" +
					$"Ожидается (Pacs.Test.Database): «{ExpectedTestDatabaseName}».");
			}

			log?.Invoke($"Подготовка фикстур (БД «{CurrentDatabaseName}»)…");
			var fixtures = LoadFixtures(author);
			log?.Invoke(
				$"Фикстуры: физ={fixtures.NaturalClientIds.Count}, юр={fixtures.LegalClientIds.Count}, " +
				$"сеть={fixtures.ChainStoreClientIds.Count}, вода={fixtures.WaterNomenclatureIds.Count}, " +
				$"интервалы={fixtures.DeliveryScheduleIds.Count}, авто={fixtures.CarIds.Count}, " +
				$"базы={fixtures.GeoGroupIds.Count}. Режим: UoW1 заказ → UoW2 МЛ (1 МЛ на заказ).");

			var sharedState = new SharedRunState();

			using(var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
			{
				var workers = Enumerable.Range(1, threadCount)
					.Select(threadId => Task.Run(
						() => WorkerLoop(threadId, fixtures, linkedCts, sharedState, log),
						linkedCts.Token))
					.ToArray();

				try
				{
					await Task.WhenAll(workers).ConfigureAwait(false);
				}
				catch(OperationCanceledException)
				{
					// штатная остановка
				}
			}

			if(sharedState.FirstError != null)
			{
				throw new AggregateException(
					$"Генерация остановлена из-за ошибки. Успешных итераций (заказ+МЛ) до остановки: {sharedState.SuccessCount}.",
					sharedState.FirstError);
			}

			log?.Invoke($"Генерация остановлена. Успешных итераций (заказ+МЛ): {sharedState.SuccessCount}.");
		}

		private void WorkerLoop(
			int threadId,
			LoadTestFixtures fixtures,
			CancellationTokenSource linkedCts,
			SharedRunState sharedState,
			Action<string> log)
		{
			var random = new Random(unchecked(Environment.TickCount * 31 + threadId * 997));

			while(!linkedCts.IsCancellationRequested)
			{
				try
				{
					InsertOrderAndRouteList(threadId, fixtures, random, linkedCts.Token);
					var successCount = Interlocked.Increment(ref sharedState.SuccessCount);
					if(successCount == 1 || successCount % 10 == 0)
					{
						log?.Invoke($"Успешных итераций (заказ+МЛ): {successCount}");
					}
				}
				catch(OperationCanceledException)
				{
					return;
				}
				catch(Exception ex)
				{
					lock(sharedState.Sync)
					{
						if(sharedState.FirstError == null)
						{
							sharedState.FirstError = ex;
							log?.Invoke($"[Поток {threadId}] ОШИБКА: {FormatException(ex)}");
							try
							{
								linkedCts.Cancel();
							}
							catch(ObjectDisposedException)
							{
								// ignore
							}
						}
					}

					return;
				}
			}
		}

		private void InsertOrderAndRouteList(
			int threadId,
			LoadTestFixtures fixtures,
			Random random,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var clientType = (ClientFixtureType)random.Next(0, 3);
			var counterpartyId = PickCounterpartyId(fixtures, clientType, random);
			var paymentType = _paymentTypes[random.Next(_paymentTypes.Length)];
			var waterId = fixtures.WaterNomenclatureIds[random.Next(fixtures.WaterNomenclatureIds.Count)];
			var bottlesCount = random.Next(1, 4);
			var deliveryScheduleId = fixtures.DeliveryScheduleIds[random.Next(fixtures.DeliveryScheduleIds.Count)];
			var carId = fixtures.CarIds[random.Next(fixtures.CarIds.Count)];
			var geoGroupId = fixtures.GeoGroupIds[random.Next(fixtures.GeoGroupIds.Count)];

			int orderId;
			DateTime orderDeliveryDate;

			// UoW 1: заказ
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot($"LoadTest order thread {threadId}"))
			{
				var author = uow.GetById<Employee>(fixtures.AuthorId)
					?? throw new InvalidOperationException($"Автор (сотрудник {fixtures.AuthorId}) не найден.");
				var counterparty = uow.GetById<Counterparty>(counterpartyId)
					?? throw new InvalidOperationException($"Контрагент {counterpartyId} не найден.");
				var deliveryPointId = PickDeliveryPointId(uow, counterpartyId, random);
				var deliveryPoint = uow.GetById<DeliveryPoint>(deliveryPointId)
					?? throw new InvalidOperationException($"ТД {deliveryPointId} не найдена.");
				var deliverySchedule = uow.GetById<DeliverySchedule>(deliveryScheduleId)
					?? throw new InvalidOperationException($"Интервал доставки {deliveryScheduleId} не найден.");
				var nomenclature = uow.GetById<Nomenclature>(waterId)
					?? throw new InvalidOperationException($"Номенклатура {waterId} не найдена.");

				cancellationToken.ThrowIfCancellationRequested();

				var now = DateTime.Now;
				var price = nomenclature.GetPrice(bottlesCount);
				orderDeliveryDate = DateTime.Today;

				var order = new Order
				{
					UoW = uow,
					Author = author,
					LastEditor = author,
					Version = now,
					LastEditedTime = now,
					BillDate = now,
					DeliverySchedule = deliverySchedule,
					BottlesReturn = bottlesCount,
					OrderStatus = OrderStatus.NewOrder,
					OrderSource = OrderSource.VodovozApp,
					OrderAddressType = clientType == ClientFixtureType.ChainStore
						? OrderAddressType.ChainStore
						: OrderAddressType.Delivery,
					Comment = $"LoadTest thread={threadId}"
				};

				SetAccessibleProperty(order, nameof(Order.Client), counterparty);
				SetAccessibleProperty(order, nameof(Order.DeliveryPoint), deliveryPoint);
				SetAccessibleProperty(order, nameof(Order.PaymentType), paymentType);
				SetAccessibleProperty(order, nameof(Order.DeliveryDate), (DateTime?)orderDeliveryDate);

				if(paymentType == PaymentType.Cash)
				{
					order.Trifle = DefaultBanknoteForReturn;
				}
				else if(paymentType == PaymentType.Terminal)
				{
					order.PaymentByTerminalSource = PaymentByTerminalSource.ByCard;
				}
				else if(paymentType == PaymentType.Cashless)
				{
					order.SignatureType = OrderSignatureType.BySeal;
				}

				var orderItem = CreateBlankOrderItem();
				SetAccessibleProperty(orderItem, nameof(OrderItem.Order), order);
				SetAccessibleProperty(orderItem, nameof(OrderItem.Nomenclature), nomenclature);
				SetAccessibleProperty(orderItem, nameof(OrderItem.Count), (decimal)bottlesCount);
				SetAccessibleProperty(orderItem, nameof(OrderItem.Price), price);
				order.ObservableOrderItems.Add(orderItem);

				try
				{
					uow.Save(order);
					uow.Commit();
					orderId = order.Id;
				}
				catch(Exception ex)
				{
					throw new InvalidOperationException(
						$"Поток {threadId}: ошибка ORM Save заказа " +
						$"(клиент={counterpartyId}, ТД={deliveryPointId}, оплата={paymentType}, вода={waterId}x{bottlesCount}).",
						ex);
				}
			}

			cancellationToken.ThrowIfCancellationRequested();

			// UoW 2: маршрутный лист на сохранённый заказ
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot($"LoadTest route list thread {threadId}"))
			{
				var order = uow.GetById<Order>(orderId)
					?? throw new InvalidOperationException($"Заказ {orderId} не найден во 2-й сессии.");
				var car = uow.GetById<Car>(carId)
					?? throw new InvalidOperationException($"Автомобиль {carId} не найден.");
				var geoGroup = uow.GetById<GeoGroup>(geoGroupId)
					?? throw new InvalidOperationException($"Часть города {geoGroupId} не найдена.");
				var logistician = uow.GetById<Employee>(fixtures.AuthorId)
					?? throw new InvalidOperationException($"Логист (сотрудник {fixtures.AuthorId}) не найден.");

				if(car.Driver == null)
				{
					throw new InvalidOperationException($"У автомобиля {carId} нет водителя.");
				}

				var now = DateTime.Now;
				var routeList = new RouteList
				{
					UoW = uow,
					Date = order.DeliveryDate ?? orderDeliveryDate,
					Status = RouteListStatus.New,
					Version = now,
					Logistician = logistician
				};

				routeList.Car = car;
				if(routeList.Driver == null)
				{
					routeList.Driver = car.Driver;
				}

				if(!routeList.GeographicGroups.Any())
				{
					routeList.ObservableGeographicGroups.Add(geoGroup);
				}

				var address = new RouteListItem(routeList, order, RouteListItemStatus.EnRoute)
				{
					WithForwarder = routeList.Forwarder != null
				};
				routeList.ObservableAddresses.Add(address);

				try
				{
					uow.Save(routeList);
					uow.Commit();
				}
				catch(Exception ex)
				{
					throw new InvalidOperationException(
						$"Поток {threadId}: ошибка ORM Save МЛ для заказа {orderId} " +
						$"(авто={carId}, база={geoGroupId}).",
						ex);
				}
			}
		}

		/// <summary>
		/// OrderItem имеет protected-конструктор.
		/// </summary>
		private static OrderItem CreateBlankOrderItem()
		{
			return (OrderItem)Activator.CreateInstance(
				typeof(OrderItem),
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
				binder: null,
				args: null,
				culture: null);
		}

		private static void SetAccessibleProperty(object target, string propertyName, object value)
		{
			var type = target.GetType();
			while(type != null)
			{
				var property = type.GetProperty(
					propertyName,
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

				if(property?.GetSetMethod(nonPublic: true) != null)
				{
					property.SetValue(target, value);
					return;
				}

				type = type.BaseType;
			}

			throw new InvalidOperationException(
				$"Не найдено set-свойство «{propertyName}» у типа {target.GetType().Name}.");
		}

		private LoadTestFixtures LoadFixtures(Employee author)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("LoadTest load fixtures"))
			{
				var naturalIds = LoadCounterpartyIds(uow, ClientFixtureType.Natural);
				var legalIds = LoadCounterpartyIds(uow, ClientFixtureType.Legal);
				var chainIds = LoadCounterpartyIds(uow, ClientFixtureType.ChainStore);

				if(!naturalIds.Any() || !legalIds.Any() || !chainIds.Any())
				{
					throw new InvalidOperationException(
						"Не найдены контрагенты для всех типов клиентов (физ / юр / сеть) с активной ТД.\n" +
						$"физ={naturalIds.Count}, юр={legalIds.Count}, сеть={chainIds.Count}.");
				}

				var waterIds = uow.Session.QueryOver<Nomenclature>()
					.Where(n => n.Category == NomenclatureCategory.water)
					.Where(n => !n.IsArchive)
					.Select(n => n.Id)
					.Take(MaxFixturePoolSize)
					.List<int>()
					.ToList();

				if(!waterIds.Any())
				{
					throw new InvalidOperationException("В тестовой БД не найдена номенклатура воды для заказов.");
				}

				var scheduleIds = uow.Session.QueryOver<DeliverySchedule>()
					.Where(s => !s.IsArchive)
					.Select(s => s.Id)
					.Take(MaxFixturePoolSize)
					.List<int>()
					.ToList();

				if(!scheduleIds.Any())
				{
					throw new InvalidOperationException("В тестовой БД нет активных интервалов доставки.");
				}

				Car carAlias = null;
				Employee driverAlias = null;
				var carIds = uow.Session.QueryOver(() => carAlias)
					.JoinAlias(() => carAlias.Driver, () => driverAlias)
					.Where(() => !carAlias.IsArchive)
					.Where(() => driverAlias.Status == EmployeeStatus.IsWorking)
					.Select(c => c.Id)
					.Take(MaxFixturePoolSize)
					.List<int>()
					.ToList();

				if(!carIds.Any())
				{
					throw new InvalidOperationException(
						"В тестовой БД нет неархивных автомобилей с работающим водителем для МЛ.");
				}

				var geoGroupIds = uow.Session.QueryOver<GeoGroup>()
					.Where(g => !g.IsArchived)
					.Select(g => g.Id)
					.Take(MaxFixturePoolSize)
					.List<int>()
					.ToList();

				if(!geoGroupIds.Any())
				{
					throw new InvalidOperationException("В тестовой БД нет частей города (баз) для МЛ.");
				}

				return new LoadTestFixtures(
					naturalIds,
					legalIds,
					chainIds,
					waterIds,
					scheduleIds,
					carIds,
					geoGroupIds,
					author.Id);
			}
		}

		private static List<int> LoadCounterpartyIds(IUnitOfWork uow, ClientFixtureType clientType)
		{
			DeliveryPoint deliveryPointAlias = null;
			Counterparty counterpartyAlias = null;

			var query = uow.Session.QueryOver(() => counterpartyAlias)
				.JoinAlias(() => counterpartyAlias.DeliveryPoints, () => deliveryPointAlias)
				.Where(() => !counterpartyAlias.IsArchive)
				.Where(() => deliveryPointAlias.IsActive);

			switch(clientType)
			{
				case ClientFixtureType.Natural:
					query.Where(() => counterpartyAlias.PersonType == PersonType.natural)
						.Where(() => !counterpartyAlias.IsChainStore);
					break;
				case ClientFixtureType.Legal:
					query.Where(() => counterpartyAlias.PersonType == PersonType.legal)
						.Where(() => !counterpartyAlias.IsChainStore);
					break;
				case ClientFixtureType.ChainStore:
					query.Where(() => counterpartyAlias.IsChainStore);
					break;
			}

			return query
				.Select(NHibernate.Criterion.Projections.Distinct(
					NHibernate.Criterion.Projections.Property(() => counterpartyAlias.Id)))
				.Take(MaxFixturePoolSize)
				.List<int>()
				.ToList();
		}

		private static int PickCounterpartyId(LoadTestFixtures fixtures, ClientFixtureType clientType, Random random)
		{
			IReadOnlyList<int> pool;
			switch(clientType)
			{
				case ClientFixtureType.Natural:
					pool = fixtures.NaturalClientIds;
					break;
				case ClientFixtureType.Legal:
					pool = fixtures.LegalClientIds;
					break;
				case ClientFixtureType.ChainStore:
					pool = fixtures.ChainStoreClientIds;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(clientType));
			}

			return pool[random.Next(pool.Count)];
		}

		private static int PickDeliveryPointId(IUnitOfWork uow, int counterpartyId, Random random)
		{
			var points = uow.Session.QueryOver<DeliveryPoint>()
				.Where(dp => dp.Counterparty.Id == counterpartyId)
				.Where(dp => dp.IsActive)
				.Select(dp => dp.Id)
				.List<int>();

			if(!points.Any())
			{
				throw new InvalidOperationException(
					$"У контрагента {counterpartyId} нет активных ТД.");
			}

			return points[random.Next(points.Count)];
		}

		private static string FormatException(Exception ex)
		{
			var sb = new StringBuilder();
			var current = ex;
			while(current != null)
			{
				if(sb.Length > 0)
				{
					sb.Append(" → ");
				}

				sb.Append(current.GetType().Name).Append(": ").Append(current.Message);
				current = current.InnerException;
			}

			return sb.ToString();
		}

		private sealed class SharedRunState
		{
			public readonly object Sync = new object();
			public Exception FirstError;
			public int SuccessCount;
		}

		private enum ClientFixtureType
		{
			Natural = 0,
			Legal = 1,
			ChainStore = 2
		}

		private sealed class LoadTestFixtures
		{
			public LoadTestFixtures(
				IReadOnlyList<int> naturalClientIds,
				IReadOnlyList<int> legalClientIds,
				IReadOnlyList<int> chainStoreClientIds,
				IReadOnlyList<int> waterNomenclatureIds,
				IReadOnlyList<int> deliveryScheduleIds,
				IReadOnlyList<int> carIds,
				IReadOnlyList<int> geoGroupIds,
				int authorId)
			{
				NaturalClientIds = naturalClientIds;
				LegalClientIds = legalClientIds;
				ChainStoreClientIds = chainStoreClientIds;
				WaterNomenclatureIds = waterNomenclatureIds;
				DeliveryScheduleIds = deliveryScheduleIds;
				CarIds = carIds;
				GeoGroupIds = geoGroupIds;
				AuthorId = authorId;
			}

			public IReadOnlyList<int> NaturalClientIds { get; }
			public IReadOnlyList<int> LegalClientIds { get; }
			public IReadOnlyList<int> ChainStoreClientIds { get; }
			public IReadOnlyList<int> WaterNomenclatureIds { get; }
			public IReadOnlyList<int> DeliveryScheduleIds { get; }
			public IReadOnlyList<int> CarIds { get; }
			public IReadOnlyList<int> GeoGroupIds { get; }
			public int AuthorId { get; }
		}
	}
}
