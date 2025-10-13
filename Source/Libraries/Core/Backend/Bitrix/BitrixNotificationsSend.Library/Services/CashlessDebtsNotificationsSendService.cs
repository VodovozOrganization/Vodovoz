using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.EntityRepositories.Nodes;

namespace BitrixNotificationsSend.Library.Services
{
	public class CashlessDebtsNotificationsSendService
	{
		private readonly OrderStatus[] _orderStatuses =
		{
			OrderStatus.Shipped,
			OrderStatus.UnloadingOnStock,
			OrderStatus.Closed
		};

		private readonly CounterpartyType[] _counterpartyTypes =
		{
			CounterpartyType.Supplier,
			CounterpartyType.Dealer,
			CounterpartyType.AdvertisingDepartmentClient
		};

		private readonly ILogger<CashlessDebtsNotificationsSendService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly ICounterpartySettings _counterpartySettings;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IBottlesRepository _bottlesRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IPaymentsRepository _paymentsRepository;

		public CashlessDebtsNotificationsSendService(
			ILogger<CashlessDebtsNotificationsSendService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrganizationSettings organizationSettings,
			ICounterpartySettings counterpartySettings,
			ICounterpartyRepository counterpartyRepository,
			IBottlesRepository bottlesRepository,
			IOrderRepository orderRepository,
			IPaymentsRepository paymentsRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_counterpartySettings = counterpartySettings ?? throw new ArgumentNullException(nameof(counterpartySettings));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_bottlesRepository = bottlesRepository ?? throw new ArgumentNullException(nameof(bottlesRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
		}

		public async Task SendNotifications(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Начало формирования данных по компаниям с долгом по безналу");

			var cashlessDebts = await GetCashlessDebts(cancellationToken);

			_logger.LogInformation(
				"Окончание формирования данных по компаниям с долгом по безналу. Количество компаний: {0}",
				cashlessDebts.Count());

			await Task.CompletedTask;
		}

		private async Task<IEnumerable<CounterpartyCashlessDebtData>> GetCashlessDebts(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(CashlessDebtsNotificationsSendService)))
			{
				return await GetGetCashlessDebtsByOrganization(uow, _organizationSettings.VodovozOrganizationId, cancellationToken);
			}
		}

		private async Task<IEnumerable<CounterpartyCashlessDebtData>> GetGetCashlessDebtsByOrganization(
			IUnitOfWork uow,
			int organizationId,
			CancellationToken cancellationToken)
		{
			var counterpartiesDebtData = new List<CounterpartyCashlessDebtData>();

			var counterpartiesNotPaidOrdersData = await GetNotPaidCashlessOrdersData(
				uow,
				organizationId,
				cancellationToken);

			var counterpartiesPaymentsData = await GetCounterpatiesPaymentsData(
				uow,
				counterpartiesNotPaidOrdersData.Keys,
				organizationId,
				cancellationToken);

			var counterpartiesBottlesDebtData = GetCounterpartiesBottlesDebtData(
				uow,
				counterpartiesNotPaidOrdersData.Keys);

			var counterpartiesEmails = GetCounterpartiesEmails(
				uow,
				counterpartiesNotPaidOrdersData.Keys);

			var counterpartiesPhones = GetCounterpartyiesPhones(
				uow,
				counterpartiesNotPaidOrdersData.Keys);

			var counterpartiesOrdersContactPhones = GetCounterpartiesOrdersContactPhones(
				uow,
				counterpartiesNotPaidOrdersData.Keys);

			foreach(var counterpartyOrdersData in counterpartiesNotPaidOrdersData)
			{
				var counterpartyId = counterpartyOrdersData.Key;
				var ordersData = counterpartyOrdersData.Value;
				var firstOrderData = ordersData.FirstOrDefault();

				counterpartiesPaymentsData.TryGetValue(counterpartyId, out var counterpartyPaymentsData);
				var counterpartyPayments = counterpartyPaymentsData?.FirstOrDefault();

				counterpartiesBottlesDebtData.TryGetValue(counterpartyId, out var counterpartyBottlesDebtData);

				counterpartiesEmails.TryGetValue(counterpartyId, out var counterpartyEmails);

				counterpartiesPhones.TryGetValue(counterpartyId, out var counterpartyPhones);

				counterpartiesOrdersContactPhones.TryGetValue(counterpartyId, out var counterpartyOrdersContactPhones);

				var couterpartyDebtData = new CounterpartyCashlessDebtData
				{
					OrganizationId = organizationId,
					OrganizationName = firstOrderData?.OrganizationName,
					CounterpartyId = counterpartyId,
					CounterpartyName = counterpartyPayments?.CounterpartyName,
					CounterpartyInn = counterpartyPayments?.CounterpartyInn,
					CounterpartyPhones = counterpartyPhones ?? Enumerable.Empty<Phone>(),
					CounterpartyOrdersContactPhones = counterpartyOrdersContactPhones ?? Enumerable.Empty<Phone>(),
					UnallocatedBalance = counterpartyPayments?.UnallocatedBalance ?? default,
					NotPaidOrdersSum = ordersData.Sum(x => x.NotPaidSum),
					PartialPaidOrdersSum = ordersData.Sum(x => x.PartialPaidSum),
					WriteOffSum = counterpartyPayments?.WriteOffSum ?? default,
					OverdueDebtorDebt = ordersData.Sum(x => x.OverdueDebtorDebt),
					DelayDaysForCounterparty = counterpartyPayments?.DelayDaysForCounterparty ?? default,
					OrderMinDeliveryDate = ordersData.Min(x => x.OrderDeliveryDate),
					IsCounterpartyLiquidating = counterpartyPayments?.IsLiquidating ?? default,
					BottlesDelivered = counterpartyBottlesDebtData?.Delivered ?? 0,
					BottlesReturned = counterpartyBottlesDebtData?.Returned ?? 0,
					Emails = counterpartyEmails ?? Enumerable.Empty<Email>()
				};

				counterpartiesDebtData.Add(couterpartyDebtData);
			}

			return counterpartiesDebtData;
		}

		private async Task<IDictionary<int, OrderPaymentsDataNode[]>> GetNotPaidCashlessOrdersData(
			IUnitOfWork uow,
			int organizationId,
			CancellationToken cancellationToken) =>
			await _orderRepository.GetNotPaidCashlessOrdersData(
				uow,
				organizationId,
				_orderStatuses,
				_counterpartyTypes,
				_counterpartySettings.CounterpartyFromTenderId,
				cancellationToken);

		private async Task<IDictionary<int, CounterpartyPaymentsDataNode[]>> GetCounterpatiesPaymentsData(
			IUnitOfWork uow,
			IEnumerable<int> counterparties,
			int organizationId,
			CancellationToken cancellationToken) =>
			await _paymentsRepository.GetCounterpatiesPaymentsData(uow, counterparties, organizationId, cancellationToken);

		private IDictionary<int, BottlesBalanceQueryResult> GetCounterpartiesBottlesDebtData(
			IUnitOfWork uow,
			IEnumerable<int> counterparties) =>
			_bottlesRepository.GetCounterpartiesBottlesDebtData(uow, counterparties);

		private IDictionary<int, Email[]> GetCounterpartiesEmails(
			IUnitOfWork uow,
			IEnumerable<int> counterparties) =>
			_counterpartyRepository.GetCounterpartyEmails(uow, counterparties);

		private IDictionary<int, Phone[]> GetCounterpartyiesPhones(
			IUnitOfWork uow,
			IEnumerable<int> counterparties) =>
			_counterpartyRepository.GetCounterpartyPhones(uow, counterparties);

		private IDictionary<int, Phone[]> GetCounterpartiesOrdersContactPhones(
			IUnitOfWork uow,
			IEnumerable<int> counterparties) =>
			_counterpartyRepository.GetCounterpartyOrdersContactPhones(uow, counterparties);
	}


	/// <summary>
	/// Представляет данные о контрагенте и его финансовых показателях
	/// </summary>
	public class CounterpartyCashlessDebtData
	{
		/// <summary>
		/// Id организации
		/// </summary>
		public int OrganizationId { get; set; }

		/// <summary>
		/// Наименование организации
		/// </summary>
		public string OrganizationName { get; set; }

		/// <summary>
		/// Id контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Наименование контрагента
		/// </summary>
		public string CounterpartyName { get; set; }

		/// <summary>
		/// ИНН контрагента
		/// </summary>
		public string CounterpartyInn { get; set; }

		public IEnumerable<Phone> CounterpartyPhones { get; set; } = Enumerable.Empty<Phone>();
		public IEnumerable<Phone> CounterpartyOrdersContactPhones { get; set; } = Enumerable.Empty<Phone>();

		/// <summary>
		/// Номер телефона контрагента
		/// </summary>
		public string PhoneNumber => CounterpartyPhones.Any()
			? string.Join(", ", CounterpartyPhones.Select(x => x.Number))
			: string.Join(", ", CounterpartyOrdersContactPhones.Select(x => x.Number));

		/// <summary>
		/// Нераспределенный баланс
		/// </summary>
		public decimal UnallocatedBalance { get; set; }

		/// <summary>
		/// Сумма неоплаченных заказов
		/// </summary>
		public decimal NotPaidOrdersSum { get; set; }

		/// <summary>
		/// Сумма частичной оплаты
		/// </summary>
		public decimal PartialPaidOrdersSum { get; set; }

		/// <summary>
		/// Возвращенный баланс
		/// </summary>
		public decimal WriteOffSum { get; set; }

		/// <summary>
		/// Общий долг
		/// </summary>
		public decimal TotalDebt => NotPaidOrdersSum - UnallocatedBalance - PartialPaidOrdersSum;

		/// <summary>
		/// Дебиторская задолженность
		/// </summary>
		public decimal DebtorDebt => NotPaidOrdersSum - UnallocatedBalance - PartialPaidOrdersSum - OverdueDebtorDebt;

		/// <summary>
		/// Просроченная дебиторская задолженность
		/// </summary>
		public decimal OverdueDebtorDebt { get; set; }

		/// <summary>
		/// Отсрочка по оплате для контрагента в днях
		/// </summary>
		public int DelayDaysForCounterparty { get; set; }

		/// <summary>
		/// Дата доставки самого давнего заказа
		/// </summary>
		public DateTime? OrderMinDeliveryDate { get; set; }

		/// <summary>
		/// Максимальное количество дней просрочки
		/// </summary>
		public int MaxDelayDays =>
			OrderMinDeliveryDate.HasValue
			? (DateTime.Now - OrderMinDeliveryDate.Value).Days
			: 0;

		/// <summary>
		/// Контрагент в статусе ликвидации
		/// </summary>
		public bool IsCounterpartyLiquidating { get; set; }

		/// <summary>
		/// Контрагент в статусе ликвидации
		/// "Да" - если ликвидирован
		/// "Нет" - если не ликвидирован
		/// </summary>
		public string LiquidationStatus =>
			IsCounterpartyLiquidating
			? "Да"
			: "Нет";

		/// <summary>
		/// Дата и время выгрузки данных
		/// </summary>
		public DateTime UnloadingDate =>
			DateTime.Today;

		public int BottlesDelivered { get; set; }
		public int BottlesReturned { get; set; }

		/// <summary>
		/// Долг по бутылям
		/// </summary>
		public int BottlesDebt => BottlesDelivered - BottlesReturned;

		/// <summary>
		/// Email адреса
		/// </summary>
		public IEnumerable<Email> Emails { get; set; }
	}
}
