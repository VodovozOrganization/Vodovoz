using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Services;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Cash
{
	public class CashReceiptRepository : ICashReceiptRepository
	{
		//ReceiptForOrderNode resultAlias = null;
		private OrderItem _orderItemAlias = null;
		private VodovozOrder _orderAlias = null;
		private CashReceipt _cashReceiptAlias = null;
		private Counterparty _counterpartyAlias = null;
		private TrueMarkCashReceiptOrder _trueMarkCashReceiptOrderAlias = null;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderParametersProvider _orderParametersProvider;

		public CashReceiptRepository(IUnitOfWorkFactory uowFactory, IOrderParametersProvider orderParametersProvider)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
		}

		private ICriterion GetPositiveSumRestriction()
		{
			var orderSumProjection = Projections.Sum(
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.Decimal, "CAST(IFNULL(?1 * ?2 - ?3, 0) AS DECIMAL(14,2))"),
					NHibernateUtil.Decimal,
					Projections.Property(() => _orderItemAlias.Count),
					Projections.Property(() => _orderItemAlias.Price),
					Projections.Property(() => _orderItemAlias.DiscountMoney)
				)
			);

			return Restrictions.Gt(orderSumProjection, 0);
		}

		private ICriterion GetPaymentTypeRestriction()
		{
			var restriction = Restrictions.Disjunction()
				.Add(() => _orderAlias.PaymentType == PaymentType.Terminal)
				.Add(Restrictions.Conjunction()
					.Add(() => _orderAlias.PaymentType == PaymentType.cash)
					.Add(() => _counterpartyAlias.AlwaysSendReceipts))
				.Add(Restrictions.Conjunction()
					.Add(() => _orderAlias.PaymentType == PaymentType.ByCard)
					.Add(Restrictions.Disjunction()
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderParametersProvider.PaymentFromTerminalId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderParametersProvider.GetPaymentByCardFromFastPaymentServiceId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderParametersProvider.GetPaymentByCardFromAvangardId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderParametersProvider.GetPaymentByCardFromSiteByQrCodeId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderParametersProvider.GetPaymentByCardFromMobileAppByQrCodeId)));
			return restriction;
		}

		private ICriterion GetSelfdeliveryRestriction()
		{
			var restriction = Restrictions.Conjunction()
				.Add(() => _orderAlias.SelfDelivery)
				.Add(() => _orderAlias.IsSelfDeliveryPaid);
			return restriction;
		}

		private ICriterion GetNotSendRestriction()
		{
			var restriction = Restrictions.Disjunction()
				.Add(Restrictions.IsNull(Projections.Property(() => _cashReceiptAlias.Id)))
				.Add(() => !_cashReceiptAlias.Sent);
			return restriction;
		}

		private ICriterion GetDeliveryDateRestriction()
		{
			var restriction = Restrictions.Where(() => _orderAlias.DeliveryDate >= DateTime.Today.AddDays(-3));
			return restriction;
		}

		private ICriterion GetMissingReceiptOrderRestriction()
		{
			var restriction = Restrictions.IsNull(
				Projections.Property(() => _trueMarkCashReceiptOrderAlias.Id)
			);
			return restriction;
		}

		private ICriterion GetAvailabilityReceiptOrderRestriction()
		{
			var restriction = Restrictions.IsNotNull(
				Projections.Property(() => _trueMarkCashReceiptOrderAlias.Id)
			);
			return restriction;
		}

		private ICriterion GetOrderStatusRestriction()
		{
			var receiptAllowedStatuses = new[] { OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };
			var restriction = Restrictions.In(Projections.Property(() => _orderAlias.OrderStatus), receiptAllowedStatuses);
			return restriction;
		}

		private ICriterion GetReceiptOrderStatusRestriction()
		{
			var receiptAllowedStatuses = new[] { TrueMarkCashReceiptOrderStatus.New, TrueMarkCashReceiptOrderStatus.CodeError };
			var restriction = Restrictions.In(Projections.Property(() => _trueMarkCashReceiptOrderAlias.Status), receiptAllowedStatuses);
			return restriction;
		}


		public IEnumerable<int> GetSelfdeliveryOrderIdsForCashReceipt()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var query = uow.Session.QueryOver(() => _orderAlias)
					.Inner.JoinAlias(() => _orderAlias.Client, () => _counterpartyAlias)
					.Left.JoinAlias(() => _orderAlias.OrderItems, () => _orderItemAlias)
					.JoinEntityAlias(() => _cashReceiptAlias, () => _cashReceiptAlias.Order.Id == _orderAlias.Id, JoinType.LeftOuterJoin)
					.JoinEntityAlias(() => _trueMarkCashReceiptOrderAlias, () => _orderAlias.Id == _trueMarkCashReceiptOrderAlias.Order.Id, JoinType.LeftOuterJoin)
					.Where(GetMissingReceiptOrderRestriction())
					.Where(GetPaymentTypeRestriction())
					.Where(GetSelfdeliveryRestriction())
					.Where(GetPositiveSumRestriction())
					.Where(GetDeliveryDateRestriction())
					.Where(GetNotSendRestriction());

				var result = query.Select(Projections.Id()).List<int>();
				return result;
			}
		}

		public IEnumerable<int> GetOrderIdsForCashReceipt(IUnitOfWork uow)
		{
			var query = uow.Session.QueryOver(() => _orderAlias)
				.Inner.JoinAlias(() => _orderAlias.Client, () => _counterpartyAlias)
				.Left.JoinAlias(() => _orderAlias.OrderItems, () => _orderItemAlias)
				.JoinEntityAlias(() => _cashReceiptAlias, () => _cashReceiptAlias.Order.Id == _orderAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => _trueMarkCashReceiptOrderAlias, () => _orderAlias.Id == _trueMarkCashReceiptOrderAlias.Order.Id, JoinType.LeftOuterJoin)
				.Where(GetAvailabilityReceiptOrderRestriction())
				.Where(GetPaymentTypeRestriction())
				.Where(GetPositiveSumRestriction())
				.Where(GetDeliveryDateRestriction())
				.Where(GetNotSendRestriction())
				.Where(GetReceiptOrderStatusRestriction())
				.Where(Restrictions.Disjunction()
					.Add(GetSelfdeliveryRestriction())
					.Add(GetOrderStatusRestriction())
				);

			var result = query.Select(Projections.Id()).List<int>();
			return result;
		}

		public bool CashReceiptNeeded(IUnitOfWork uow, int orderId)
		{
			var query = uow.Session.QueryOver(() => _orderAlias)
				.Inner.JoinAlias(() => _orderAlias.Client, () => _counterpartyAlias)
				.Left.JoinAlias(() => _orderAlias.OrderItems, () => _orderItemAlias)
				.JoinEntityAlias(() => _cashReceiptAlias, () => _cashReceiptAlias.Order.Id == _orderAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => _trueMarkCashReceiptOrderAlias, () => _orderAlias.Id == _trueMarkCashReceiptOrderAlias.Order.Id, JoinType.LeftOuterJoin)
				.Where(() => _orderAlias.Id == orderId)
				.Where(GetAvailabilityReceiptOrderRestriction())
				.Where(GetPaymentTypeRestriction())
				.Where(GetPositiveSumRestriction())
				.Where(GetDeliveryDateRestriction())
				.Where(Restrictions.Disjunction()
					.Add(GetSelfdeliveryRestriction())
					.Add(GetOrderStatusRestriction())
				);

			var id = query.Select(Projections.Id()).SingleOrDefault<int>();
			return id == orderId;
		}

		public TrueMarkCashReceiptOrder LoadReceipt(IUnitOfWork uow, int receiptId)
		{
			TrueMarkCashReceiptOrder receiptAlias = null;
			TrueMarkCashReceiptProductCode productCodeAlias = null;

			IQueryOver<TrueMarkCashReceiptOrder, TrueMarkCashReceiptOrder> CreateLastOrdersBaseQuery()
			{
				var baseQuery = uow.Session.QueryOver(() => receiptAlias)
					.Where(() => receiptAlias.Id == receiptId);
				return baseQuery;
			}

			// Порядок составления запросов важен.
			// Запрос построен так, чтобы в одном обращении к базе были загружены
			// все используемые поля

			var receiptQuery = CreateLastOrdersBaseQuery()
				.Future<TrueMarkCashReceiptOrder>();

			var orderQuery = CreateLastOrdersBaseQuery()
				.Fetch(SelectMode.Fetch, () => receiptAlias.Order)
				.Future<TrueMarkCashReceiptOrder>();

			var counterpartyQuery = CreateLastOrdersBaseQuery()
				.Fetch(SelectMode.Fetch, () => receiptAlias.Order.Client)
				.Future<TrueMarkCashReceiptOrder>();

			var codesQuery = CreateLastOrdersBaseQuery()
				.Left.JoinAlias(() => receiptAlias.ScannedCodes, () => productCodeAlias)
				.Fetch(SelectMode.Fetch, () => productCodeAlias.SourceCode)
				.Fetch(SelectMode.Fetch, () => productCodeAlias.ResultCode)
				.Future<TrueMarkCashReceiptOrder>();

			var result = codesQuery.SingleOrDefault<TrueMarkCashReceiptOrder>();
			return result;
		}
	}

	//public class ReceiptForOrderNode
	//{
	//	public int OrderId { get; set; }
	//	public int TrueMarkCashReceiptOrderId { get; set; }
	//	public int? ReceiptId { get; set; }
	//	public bool? WasSent { get; set; }
	//}
}
