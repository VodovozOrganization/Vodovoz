using QS.DomainModel.UoW;
using QS.Project.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.AdministrationTools.OrdersLoadTesting
{
	/// <summary>
	/// Нагрузочный тест: параллельная массовая вставка заказов через NHibernate
	/// без бизнес-логики (без AcceptOrder / договоров / МЛ).
	/// Цель — поймать блокировки БД при конкурентных INSERT.
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
				$"интервалы={fixtures.DeliveryScheduleIds.Count}. Режим: только NH INSERT заказов.");

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
					$"Генерация остановлена из-за ошибки. Успешных вставок до остановки: {sharedState.SuccessCount}.",
					sharedState.FirstError);
			}

			log?.Invoke($"Генерация остановлена. Успешных вставок: {sharedState.SuccessCount}.");
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
					InsertOrder(threadId, fixtures, random, linkedCts.Token);
					var successCount = Interlocked.Increment(ref sharedState.SuccessCount);
					if(successCount == 1 || successCount % 10 == 0)
					{
						log?.Invoke($"Успешных вставок: {successCount}");
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

		private void InsertOrder(
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

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot($"LoadTest insert thread {threadId}"))
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
				var order = new Order
				{
					UoW = uow,
					Author = author,
					LastEditor = author,
					Version = now,
					LastEditedTime = now,
					DeliverySchedule = deliverySchedule,
					BottlesReturn = bottlesCount,
					Comment = $"LoadTest thread={threadId}"
				};

				// Первичная установка клиента/ТД/даты без UpdateContract (old* == null / Contract == null).
				// Updater передаём null — ветки с вызовом контракта не должны сработать.
				order.UpdateClient(counterparty, null, out _);
				order.UpdateDeliveryPoint(deliveryPoint, null);
				order.UpdatePaymentType(paymentType, null, needUpdateContract: false);
				order.UpdateDeliveryDate(DateTime.Today, null, out _);

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

				var price = nomenclature.GetPrice(bottlesCount);
				var orderItem = OrderLoadTestItemFactory.CreateSaleItem(order, nomenclature, bottlesCount, price);
				order.ObservableOrderItems.Add(orderItem);

				try
				{
					uow.Save(order);
					uow.Commit();
				}
				catch(Exception ex)
				{
					throw new InvalidOperationException(
						$"Поток {threadId}: ошибка NH INSERT заказа " +
						$"(клиент={counterpartyId}, ТД={deliveryPointId}, оплата={paymentType}, вода={waterId}x{bottlesCount}).",
						ex);
				}
			}
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

				return new LoadTestFixtures(
					naturalIds,
					legalIds,
					chainIds,
					waterIds,
					scheduleIds,
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
				int authorId)
			{
				NaturalClientIds = naturalClientIds;
				LegalClientIds = legalClientIds;
				ChainStoreClientIds = chainStoreClientIds;
				WaterNomenclatureIds = waterNomenclatureIds;
				DeliveryScheduleIds = deliveryScheduleIds;
				AuthorId = authorId;
			}

			public IReadOnlyList<int> NaturalClientIds { get; }
			public IReadOnlyList<int> LegalClientIds { get; }
			public IReadOnlyList<int> ChainStoreClientIds { get; }
			public IReadOnlyList<int> WaterNomenclatureIds { get; }
			public IReadOnlyList<int> DeliveryScheduleIds { get; }
			public int AuthorId { get; }
		}
	}
}
