using BitrixNotificationsSend.Client;
using BitrixNotificationsSend.Contracts.Dto;
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
		private readonly IBitrixNotificationsSendClient _bitrixNotificationsSendClient;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IBottlesRepository _bottlesRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IPaymentsRepository _paymentsRepository;

		public CashlessDebtsNotificationsSendService(
			ILogger<CashlessDebtsNotificationsSendService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IBitrixNotificationsSendClient bitrixNotificationsSendClient,
			IOrganizationSettings organizationSettings,
			ICounterpartyRepository counterpartyRepository,
			IBottlesRepository bottlesRepository,
			IOrderRepository orderRepository,
			IPaymentsRepository paymentsRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_bitrixNotificationsSendClient = bitrixNotificationsSendClient ?? throw new ArgumentNullException(nameof(bitrixNotificationsSendClient));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_bottlesRepository = bottlesRepository ?? throw new ArgumentNullException(nameof(bottlesRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
		}

		public async Task SendNotifications(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Начало формирования данных по компаниям с долгом по безналу");

			var cashlessDebts =
				await GetCashlessDebts(cancellationToken);

			_logger.LogInformation(
				"Окончание формирования данных по компаниям с долгом по безналу. Количество компаний: {0}",
				cashlessDebts.Count());

			_logger.LogInformation("Начало отправки уведомлений по долгам по безналу в Битрикс24");

			var sendNotificationResult =
				await _bitrixNotificationsSendClient.SendCounterpartiesCashlessDebtsNotification(cashlessDebts, cancellationToken);

			if(sendNotificationResult.IsSuccess)
			{
				_logger.LogInformation("Уведомления по долгам по безналу успешно отправлены в Битрикс24");
			}
			else
			{
				var message = sendNotificationResult.Errors.FirstOrDefault()?.Message;
				_logger.LogError(message);
			}

			await Task.CompletedTask;
		}

		private async Task<IEnumerable<CounterpartyCashlessDebtDto>> GetCashlessDebts(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(CashlessDebtsNotificationsSendService)))
			{
				return await GetGetCashlessDebtsByOrganization(uow, _organizationSettings.VodovozOrganizationId, cancellationToken);
			}
		}

		private async Task<IEnumerable<CounterpartyCashlessDebtDto>> GetGetCashlessDebtsByOrganization(
			IUnitOfWork uow,
			int organizationId,
			CancellationToken cancellationToken)
		{
			var counterpartiesDebtData = new List<CounterpartyCashlessDebtDto>();

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

				var couterpartyDebtData = new CounterpartyCashlessDebtDto
				{
					OrganizationId = organizationId,
					OrganizationName = firstOrderData?.OrganizationName,
					CounterpartyId = counterpartyId,
					CounterpartyName = counterpartyPayments?.CounterpartyName,
					CounterpartyInn = counterpartyPayments?.CounterpartyInn,
					CounterpartyPhones = counterpartyPhones?.Select(x => x.Number).Distinct() ?? Enumerable.Empty<string>(),
					CounterpartyOrdersContactPhones = counterpartyOrdersContactPhones?.Select(x => x.Number).Distinct() ?? Enumerable.Empty<string>(),
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
					Emails = counterpartyEmails?.Select(x => x.Address).Distinct() ?? Enumerable.Empty<string>()
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
}
