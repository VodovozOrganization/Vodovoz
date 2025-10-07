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
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace BitrixNotificationsSend.Library.Services
{
	public class CashlessDebtsNotificationsSendService
	{
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
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(CashlessDebtsNotificationsSendService)))
			{
				var query =
					from order in uow.Session.Query<OrderEntity>()
					join client in uow.Session.Query<CounterpartyEntity>() on order.Client.Id equals client.Id
					where
						order.OrderPaymentStatus != OrderPaymentStatus.Paid
						&& order.PaymentType == PaymentType.Cashless
						&& client.PersonType == PersonType.legal
					select order;

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
		public int UnpaidOrdersSum { get; set; }

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
	}
}
