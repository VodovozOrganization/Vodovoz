using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Operations;
using VodovozBusiness.Domain.Payments;

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

		public CashlessDebtsNotificationsSendService(
			ILogger<CashlessDebtsNotificationsSendService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrganizationSettings organizationSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
		}

		public async Task SendNotifications(CancellationToken cancellationToken)
		{
			var cashlessDebts = await GetCashlessDebts(cancellationToken);

			await Task.CompletedTask;
		}

		private async Task<IEnumerable<CounterpartyCashlessDebtData>> GetCashlessDebts(CancellationToken cancellationToken)
		{
			var today = DateTime.Today;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(CashlessDebtsNotificationsSendService)))
			{
				var notPaidOrdersData = await GetNotPaidOrdersData(
					uow,
					_organizationSettings.VodovozOrganizationId,
					cancellationToken);

				var counterpartyPaymentsData = await GetCounterpatyPaymentsData(
					uow,
					notPaidOrdersData.Keys,
					_organizationSettings.VodovozOrganizationId,
					cancellationToken);

				return Enumerable.Empty<CounterpartyCashlessDebtData>();
			}
		}

		private async Task<IDictionary<int, OrderData[]>> GetNotPaidOrdersData(
			IUnitOfWork uow,
			int organizationId,
			CancellationToken cancellationToken)
		{
			var today = DateTime.Today;

			var ordersDataQuery =
					from order in uow.Session.Query<Order>()
					join counterparty in uow.Session.Query<Counterparty>() on order.Client.Id equals counterparty.Id
					join cc in uow.Session.Query<CounterpartyContract>() on order.Contract.Id equals cc.Id into contacts
					from contract in contacts.DefaultIfEmpty()
					join o in uow.Session.Query<Organization>() on contract.Organization.Id equals o.Id into organizations
					from organization in organizations.DefaultIfEmpty()
					join ccf in uow.Session.Query<ClientCameFrom>() on counterparty.CameFrom.Id equals ccf.Id into camefroms
					from clientCameFrom in camefroms.DefaultIfEmpty()

					let notPaidOrdersSum =
					(decimal?)(from orderItem in uow.Session.Query<OrderItem>()
							   where
							   orderItem.Order.Id == order.Id
							   select
							   orderItem.ActualSum)
							   .Sum() ?? 0

					let patrialPaidOrdersSum =
					(decimal?)(from paymentItem in uow.Session.Query<PaymentItem>()
							   join cashlessMovementOpetation in uow.Session.Query<CashlessMovementOperation>()
									on paymentItem.CashlessMovementOperation.Id equals cashlessMovementOpetation.Id
							   where
							   paymentItem.Order.Id == order.Id
							   && cashlessMovementOpetation.CashlessMovementOperationStatus != AllocationStatus.Cancelled
							   select cashlessMovementOpetation.Expense)
							   .Sum() ?? 0

					let overdueDebtorDebt =
					(decimal?)(from orderItem in uow.Session.Query<OrderItem>()
							   where
							   orderItem.Order.Id == order.Id
							   && order.DeliveryDate != null
							   && order.DeliveryDate.Value.AddDays(counterparty.DelayDaysForBuyers) < today
							   select
							   orderItem.ActualSum)
							   .Sum() ?? 0

					let orderSum =
					(decimal?)(from orderItem in uow.Session.Query<OrderItem>()
							   where
							   orderItem.Order.Id == order.Id
							   select
							   orderItem.ActualSum)
							   .Sum() ?? 0

					let bottlesDelivered =
					(int?)(from bottleMovementOperation in uow.Session.Query<BottlesMovementOperation>()
						   where
						   bottleMovementOperation.Counterparty.Id == counterparty.Id
						   select
						   bottleMovementOperation.Delivered)
						   .Sum() ?? 0

					let bottlesReturned =
					(int?)(from bottleMovementOperation in uow.Session.Query<BottlesMovementOperation>()
						   where
						   bottleMovementOperation.Counterparty.Id == counterparty.Id
						   select
						   bottleMovementOperation.Returned)
						   .Sum() ?? 0

					let counterpartyPhones =
					from phone in uow.Session.Query<Phone>()
					where phone.Counterparty.Id == counterparty.Id
					select phone

					let counterpartyOrdersContactPhones =
					from order in uow.Session.Query<Order>()
					join phone in uow.Session.Query<Phone>() on order.ContactPhone.Id equals phone.Id
					where
					order.Client.Id == counterparty.Id
					select phone

					let isExpired =
						order.DeliveryDate != null
						&& order.DeliveryDate.Value.AddDays(counterparty.DelayDaysForBuyers) < today

					where
						order.OrderPaymentStatus != OrderPaymentStatus.Paid
						&& _orderStatuses.Contains(order.OrderStatus)
						&& order.PaymentType == PaymentType.Cashless
						&& counterparty.PersonType == PersonType.legal
						&& _counterpartyTypes.Contains(counterparty.CounterpartyType)
						&& organization.Id == organizationId
						&& counterparty.CloseDeliveryDebtType == null
						&& order.DeliveryDate != null
						&& orderSum > 0
						&& (clientCameFrom == null || clientCameFrom.Name != "Тендер")
						&& !counterparty.IsChainStore
						&& isExpired

					select new OrderData
					{
						OrderId = order.Id,
						CounterpartyId = counterparty.Id,
						OrganizationId = organization.Id,
						OrganizationName = organization.FullName,
						NotPaidSum = notPaidOrdersSum,
						PartialPaidSum = patrialPaidOrdersSum,
						OverdueDebtorDebt = overdueDebtorDebt,
						OrderDeliveryDate = order.DeliveryDate,
						BottlesDelivered = bottlesDelivered,
						BottlesReturned = bottlesReturned
					};

			var notPaidOrdersData =
				(await ordersDataQuery.ToListAsync(cancellationToken))
				.GroupBy(x => x.CounterpartyId)
				.ToDictionary(
					x => x.Key,
					x => x.ToArray());

			return notPaidOrdersData;
		}

		private async Task<IDictionary<int, CounterpartyPaymentsData[]>> GetCounterpatyPaymentsData(
			IUnitOfWork uow,
			IEnumerable<int> counterparties,
			int organizationId,
			CancellationToken cancellationToken)
		{
			var query =
				from counterparty in uow.Session.Query<Counterparty>()

				let counterpartyIncomeSum =
				(decimal?)(from cashlessMovementOperations in uow.Session.Query<CashlessMovementOperation>()
						   where
						   cashlessMovementOperations.Counterparty.Id == counterparty.Id
						   && cashlessMovementOperations.CashlessMovementOperationStatus != AllocationStatus.Cancelled
						   && cashlessMovementOperations.Organization.Id == organizationId
						   select cashlessMovementOperations.Income)
						   .Sum() ?? 0

				let counterpartyPaymentItemsSum =
				(decimal?)(from paymentItem in uow.Session.Query<PaymentItem>()
						   join payment in uow.Session.Query<Payment>() on paymentItem.Payment.Id equals payment.Id
						   where
						   payment.Counterparty.Id == counterparty.Id
						   && paymentItem.PaymentItemStatus != AllocationStatus.Cancelled
						   && payment.Organization.Id == organizationId
						   select paymentItem.Sum)
						   .Sum() ?? 0

				let paymentsWriteOffSum =
				(decimal?)(from paymentWriteOff in uow.Session.Query<PaymentWriteOff>()
						   join cashlessMovementOperations in uow.Session.Query<CashlessMovementOperation>()
								on paymentWriteOff.CashlessMovementOperation.Id equals cashlessMovementOperations.Id
						   where
							   paymentWriteOff.CounterpartyId == counterparty.Id
							   && paymentWriteOff.OrganizationId != null
							   && organizationId == paymentWriteOff.OrganizationId.Value
							   && cashlessMovementOperations.CashlessMovementOperationStatus != AllocationStatus.Cancelled
						   select paymentWriteOff.Sum)
						   .Sum() ?? 0

				where counterparties.Contains(counterparty.Id)

				select new CounterpartyPaymentsData
				{
					CounterpartyId = counterparty.Id,
					CounterpartyName = counterparty.Name,
					CounterpartyInn = counterparty.INN,
					IncomeSum = counterpartyIncomeSum,
					PaymentItemsSum = counterpartyPaymentItemsSum,
					WriteOffSum = paymentsWriteOffSum,
					DelayDaysForCounterparty = counterparty.DelayDaysForBuyers,
					IsLiquidating = counterparty.IsLiquidating,
				};

			var counterpartyPaymentsData =
				(await query.ToListAsync(cancellationToken))
				.GroupBy(x => x.CounterpartyId)
				.ToDictionary(
					x => x.Key,
					x => x.ToArray());

			return counterpartyPaymentsData;
		}
	}

	public class OrderData
	{
		public int OrderId { get; set; }
		public int CounterpartyId { get; set; }
		public int OrganizationId { get; set; }
		public string OrganizationName { get; set; }
		public decimal NotPaidSum { get; set; }
		public decimal PartialPaidSum { get; set; }
		public decimal OverdueDebtorDebt { get; set; }
		public DateTime? OrderDeliveryDate { get; set; }
		public int BottlesDelivered { get; set; }
		public int BottlesReturned { get; set; }
	}

	public class CounterpartyPaymentsData
	{
		public int CounterpartyId { get; set; }
		public string CounterpartyName { get; set; }
		public string CounterpartyInn { get; set; }
		public decimal IncomeSum { get; set; }
		public decimal PaymentItemsSum { get; set; }
		public decimal UnallocatedBalance => IncomeSum - PaymentItemsSum;
		public decimal WriteOffSum { get; set; }
		public int DelayDaysForCounterparty { get; set; }
		public bool IsLiquidating { get; set; }
	}


	/// <summary>
	/// Представляет данные о контрагенте и его финансовых показателях
	/// </summary>
	public class CounterpartyCashlessDebtData
	{
		public int OrderId { get; set; }
		public int OrganizationId { get; set; }
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
		/// Наименование организации
		/// </summary>
		public string Organization { get; set; }

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
		/// Дата доставки заказа
		/// </summary>
		public DateTime? OrderDeliveryDate { get; set; }

		/// <summary>
		/// Максимальное количество дней просрочки
		/// </summary>
		public int MaxDelayDays =>
			OrderDeliveryDate.HasValue
			? (DateTime.Now - OrderDeliveryDate.Value).Days
			: 0;

		/// <summary>
		/// Статус ликвидации организации
		/// </summary>
		public string LiquidationStatus { get; set; }

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
		public string EmailAdresses { get; set; }
	}
}
