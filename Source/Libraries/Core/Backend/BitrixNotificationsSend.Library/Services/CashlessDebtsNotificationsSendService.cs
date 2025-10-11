using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

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

		public CashlessDebtsNotificationsSendService(
			ILogger<CashlessDebtsNotificationsSendService> logger,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
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
				var query =
					from order in uow.Session.Query<OrderEntity>()
					join counterparty in uow.Session.Query<CounterpartyEntity>() on order.Client.Id equals counterparty.Id
					join contract in uow.Session.Query<CounterpartyContractEntity>() on order.Contract.Id equals contract.Id
					join organization in uow.Session.Query<OrganizationEntity>() on contract.Organization.Id equals organization.Id

					let counterpartyIncomeSum =
					(decimal?)(from cashlessMovementOperations in uow.Session.Query<CashlessMovementOperationEntity>()
							   where
							   cashlessMovementOperations.Counterparty.Id == counterparty.Id
							   && cashlessMovementOperations.CashlessMovementOperationStatus != AllocationStatus.Cancelled
							   select cashlessMovementOperations.Income)
							   .Sum() ?? 0

					let counterpartyPaymentItemsSum =
					(decimal?)(from paymentItem in uow.Session.Query<PaymentItemEntity>()
							   join payment in uow.Session.Query<PaymentEntity>() on paymentItem.Payment.Id equals payment.Id
							   where
							   payment.Counterparty.Id == counterparty.Id
							   && paymentItem.PaymentItemStatus != AllocationStatus.Cancelled
							   select paymentItem.Sum)
							   .Sum() ?? 0

					let notPaidOrdersSum =
					(decimal?)(from orderItem in uow.Session.Query<OrderItemEntity>()
							   where
							   orderItem.Order.Id == order.Id
							   select
							   orderItem.Price * (orderItem.ActualCount ?? orderItem.Count) - orderItem.DiscountMoney)
							   .Sum() ?? 0

					let patrialPaidOrdersSum =
					(decimal?)(from paymentItem in uow.Session.Query<PaymentItemEntity>()
							   join cashlessMovementOpetation in uow.Session.Query<CashlessMovementOperationEntity>()
									on paymentItem.CashlessMovementOperation.Id equals cashlessMovementOpetation.Id
							   where
							   paymentItem.Order.Id == order.Id
							   && cashlessMovementOpetation.CashlessMovementOperationStatus != AllocationStatus.Cancelled
							   select cashlessMovementOpetation.Expense)
							   .Sum() ?? 0

					let isExpired =
						order.DeliveryDate != null
						&& order.DeliveryDate.Value.AddDays(counterparty.DelayDaysForBuyers) <= today


					where
						order.OrderPaymentStatus != OrderPaymentStatus.Paid
						&& _orderStatuses.Contains(order.OrderStatus)
						&& order.PaymentType == PaymentType.Cashless
						&& counterparty.PersonType == PersonType.legal
						&& _counterpartyTypes.Contains(counterparty.CounterpartyType)
						//&& isExpired
					
					select new CounterpartyCashlessDebtData
					{
						OrderId = order.Id,
						CounterpartyName = counterparty.Name,
						CounterpartyInn = counterparty.INN,
						PhoneNumber = string.Empty,
						Organization = organization.FullName,
						UnallocatedBalance = counterpartyIncomeSum - counterpartyPaymentItemsSum,
						UnpaidOrdersSum = notPaidOrdersSum,
						PatrialPaidOrdersSum = patrialPaidOrdersSum
						//WriteOffSum = counterparty.WriteOffSum,
						//TotalDebt = counterparty.Debt,
						//DebtorDebt = counterparty.Debt > 0 ? counterparty.Debt : 0,
						//OverdueDebtorDebt = counterparty.OverdueDebt > 0 ? counterparty.OverdueDebt : 0,
						//DelayDaysForCounterparty = counterparty.DelayDaysForCounterparty,
						//MaxDelayDays = uow.Session.Query<OrderEntity>()
						//	.Where(o => o.Client.Id == counterparty.Id && o.DeliveryDate < DateTime.Now && o.OrderPaymentStatus != OrderPaymentStatus.Paid)
						//	.Select(o => (int?)(DateTime.Now - o.DeliveryDate).Days)
						//	.Max() ?? 0,
						//TypeDebt = counterparty.Debt > 0 ? (counterparty.OverdueDebt > 0 ? "Просроченная" : "Текущая") : "Отсутствует",
						//LiquidationStatus = counterparty.LiquidationStatus.GetEnumTitle(),
						//UnloadingDate = DateTime.Now,
						//BottlesDebt = counterparty.BottlesDebt,
						//EmailAdresses = string.Join(", ", counterparty.Emails)
					};

				var orders = await query.ToListAsync(cancellationToken);

				return Enumerable.Empty<CounterpartyCashlessDebtData>();
			}
		}
	}

	/// <summary>
	/// Представляет данные о контрагенте и его финансовых показателях
	/// </summary>
	public class CounterpartyCashlessDebtData
	{
		public int OrderId { get; set; }
		/// <summary>
		/// Наименование контрагента
		/// </summary>
		public string CounterpartyName { get; set; }

		/// <summary>
		/// ИНН контрагента
		/// </summary>
		public string CounterpartyInn { get; set; }

		/// <summary>
		/// Номер телефона контрагента
		/// </summary>
		public string PhoneNumber { get; set; }

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
		public decimal UnpaidOrdersSum { get; set; }

		/// <summary>
		/// Сумма частичной оплаты
		/// </summary>
		public decimal PatrialPaidOrdersSum { get; set; }

		/// <summary>
		/// Возвращенный баланс
		/// </summary>
		public decimal WriteOffSum { get; set; }

		/// <summary>
		/// Общий долг
		/// </summary>
		public decimal TotalDebt { get; set; }

		/// <summary>
		/// Дебиторская задолженность
		/// </summary>
		public decimal DebtorDebt { get; set; }

		/// <summary>
		/// Просроченная дебиторская задолженность
		/// </summary>
		public decimal OverdueDebtorDebt { get; set; }

		/// <summary>
		/// Отсрочка по оплате для контрагента в днях
		/// </summary>
		public int DelayDaysForCounterparty { get; set; }

		/// <summary>
		/// Максимальное количество дней просрочки
		/// </summary>
		public int MaxDelayDays { get; set; }

		/// <summary>
		/// Тип задолженности
		/// </summary>
		public string TypeDebt { get; set; }

		/// <summary>
		/// Статус ликвидации организации
		/// </summary>
		public string LiquidationStatus { get; set; }

		/// <summary>
		/// Дата и время выгрузки данных
		/// </summary>
		public DateTime UnloadingDate { get; set; }

		/// <summary>
		/// Долг по бутылям
		/// </summary>
		public int BottlesDebt { get; set; }

		/// <summary>
		/// Email адреса
		/// </summary>
		public string EmailAdresses { get; set; }
	}
}
