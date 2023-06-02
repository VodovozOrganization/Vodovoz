using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NPOI.SS.Formula.Functions;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Services;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Cash
{
	public class CashReceiptRepository : ICashReceiptRepository
	{
		private OrderItem _orderItemAlias = null;
		private VodovozOrder _orderAlias = null;
		private CashReceipt _cashReceiptAlias = null;
		private Counterparty _counterpartyAlias = null;
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
				.Add(() => _orderAlias.PaymentType == PaymentType.Cash)
				.Add(() => _orderAlias.PaymentType == PaymentType.DriverApplicationQR)
				.Add(() => _orderAlias.PaymentType == PaymentType.SmsQR)
				.Add(Restrictions.Conjunction()
					.Add(() => _orderAlias.PaymentType == PaymentType.PaidOnline)
					.Add(Restrictions.Disjunction()
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderParametersProvider.PaymentFromTerminalId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderParametersProvider.GetPaymentByCardFromFastPaymentServiceId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderParametersProvider.GetPaymentByCardFromAvangardId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderParametersProvider.GetPaymentByCardFromSiteByQrCodeId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderParametersProvider.GetPaymentByCardFromMobileAppByQrCodeId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderParametersProvider.PaymentByCardFromMobileAppId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderParametersProvider.PaymentByCardFromSiteId)
						)
					);
			return restriction;
		}

		private ICriterion GetSelfdeliveryRestriction()
		{
			var restriction = Restrictions.Conjunction()
				.Add(() => _orderAlias.SelfDelivery)
				.Add(() => _orderAlias.IsSelfDeliveryPaid);
			return restriction;
		}

		private ICriterion GetDeliveryDateRestriction()
		{
			var restriction = Restrictions.Where(() => _orderAlias.DeliveryDate >= DateTime.Today.AddDays(-3));
			return restriction;
		}

		private ICriterion GetMissingCashReceiptRestriction()
		{
			var restriction = Restrictions.IsNull(
				Projections.Property(() => _cashReceiptAlias.Id)
			);
			return restriction;
		}

		private ICriterion GetCashReceiptExistRestriction()
		{
			var restriction = Restrictions.IsNotNull(
				Projections.Property(() => _cashReceiptAlias.Id)
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
			var receiptAllowedStatuses = new[] { CashReceiptStatus.New, CashReceiptStatus.CodeError };
			var restriction = Restrictions.In(Projections.Property(() => _cashReceiptAlias.Status), receiptAllowedStatuses);
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
					.Where(GetMissingCashReceiptRestriction())
					.Where(GetPaymentTypeRestriction())
					.Where(GetSelfdeliveryRestriction())
					.Where(GetPositiveSumRestriction())
					.Where(GetDeliveryDateRestriction())
					;

				var result = query.Select(Projections.Id()).List<int>();
				return result;
			}
		}
		
		/// <summary>
		/// Получение Id доставляемых заказов, которые удовлетворяют условиям, но на них не были созданы чеки
		/// (как вариант: заказ закрыт из программы ДВ, а не из водительского приложения)
		/// </summary>
		/// <returns>Id's заказов</returns>
		public IEnumerable<int> GetDeliveryOrderIdsForCashReceipt()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var query = uow.Session.QueryOver(() => _orderAlias)
					.Inner.JoinAlias(() => _orderAlias.Client, () => _counterpartyAlias)
					.Left.JoinAlias(() => _orderAlias.OrderItems, () => _orderItemAlias)
					.JoinEntityAlias(() => _cashReceiptAlias, () => _cashReceiptAlias.Order.Id == _orderAlias.Id, JoinType.LeftOuterJoin)
					.Where(GetMissingCashReceiptRestriction())
					.And(GetPaymentTypeRestriction())
					.And(GetPositiveSumRestriction())
					.And(GetDeliveryDateRestriction())
					.And(GetOrderStatusRestriction())
					.And(() => !_orderAlias.SelfDelivery);

				var result = query.Select(Projections.Id()).List<int>();
				return result;
			}
		}

		public IEnumerable<CashReceipt> GetCashReceiptsForSend(IUnitOfWork uow, int count)
		{
			var statusesForSend = new[] { CashReceiptStatus.ReadyToSend, CashReceiptStatus.ReceiptSendError };
			var query = uow.Session.QueryOver(() => _cashReceiptAlias)
				.WhereRestrictionOn(() => _cashReceiptAlias.Status).IsIn(statusesForSend)
				.Select(Projections.Id())
				.OrderBy(() => _cashReceiptAlias.CreateDate).Asc
				.Take(count);

			var receiptIds = query.List<int>();

			var result = LoadReceipts(uow, receiptIds);
			return result;
		}

		public bool CashReceiptNeeded(IUnitOfWork uow, int orderId)
		{
			var query = uow.Session.QueryOver(() => _orderAlias)
				.Inner.JoinAlias(() => _orderAlias.Client, () => _counterpartyAlias)
				.Left.JoinAlias(() => _orderAlias.OrderItems, () => _orderItemAlias)
				.JoinEntityAlias(() => _cashReceiptAlias, () => _cashReceiptAlias.Order.Id == _orderAlias.Id, JoinType.LeftOuterJoin)
				.Where(() => _orderAlias.Id == orderId)
				.Where(GetCashReceiptExistRestriction())
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

		public bool CashReceiptNeededForFirstCashSum(IUnitOfWork uow, int orderId)
		{
			var query = uow.Session.QueryOver(() => _orderAlias)
				.Inner.JoinAlias(() => _orderAlias.Client, () => _counterpartyAlias)
				.Left.JoinAlias(() => _orderAlias.OrderItems, () => _orderItemAlias)
				.JoinEntityAlias(() => _cashReceiptAlias, () => _cashReceiptAlias.Order.Id == _orderAlias.Id, JoinType.LeftOuterJoin)
				.Where(() => _orderAlias.Id == orderId)
				.Where(GetCashReceiptExistRestriction())
				.Where(() => _orderAlias.PaymentType == PaymentType.Cash)
				.Where(GetPositiveSumRestriction())
				.Where(GetDeliveryDateRestriction())
				.Where(Restrictions.Disjunction()
					.Add(GetSelfdeliveryRestriction())
					.Add(GetOrderStatusRestriction())
				);

			var id = query.Select(Projections.Id()).SingleOrDefault<int>();
			return id == orderId;
		}

		public CashReceipt LoadReceipt(IUnitOfWork uow, int receiptId)
		{
			var receiptIds = new[] { receiptId };
			var receipts = LoadReceipts(uow, receiptIds);
			var result = receipts.SingleOrDefault();
			return result;
		}

		public IEnumerable<CashReceipt> LoadReceipts(IUnitOfWork uow, IEnumerable<int> receiptId)
		{
			CashReceipt receiptAlias = null;
			VodovozOrder orderAlias = null;
			MeasurementUnits measurementUnitsAlias = null;
			TrueMarkWaterIdentificationCode waterSourceCodeAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			DeliveryPointCategory deliveryPointCategoryAlias = null;
			CounterpartyContract contractAlias = null;
			Organization organizationAlias = null;
			CashReceiptProductCode productCodeAlias = null;

			var receiptIdsArray = receiptId.ToArray();

			var receiptRestriction = Restrictions.In(Projections.Property(() => receiptAlias.Id), receiptIdsArray);

			IQueryOver<CashReceipt, CashReceipt> CreateLastOrdersBaseQuery()
			{
				var baseQuery = uow.Session.QueryOver(() => receiptAlias)
					.WhereRestrictionOn(() => receiptAlias.Id).IsIn(receiptIdsArray);
				return baseQuery;
			}

			var receiptQuery = CreateLastOrdersBaseQuery()
				.Future<CashReceipt>();

			var measurementUnitsQuery = uow.Session.QueryOver(() => measurementUnitsAlias)
				.Future<MeasurementUnits>();

			var deliveryPointCategoryQuery = uow.Session.QueryOver(() => deliveryPointCategoryAlias)
				.Future<DeliveryPointCategory>();

			 var codesQuery = uow.Session.QueryOver(() => receiptAlias)
				.Left.JoinAlias(() => receiptAlias.ScannedCodes, () => productCodeAlias)
				.Where(receiptRestriction)
				.Fetch(SelectMode.Fetch, () => productCodeAlias.SourceCode)
				.Fetch(SelectMode.Fetch, () => productCodeAlias.ResultCode)
				.Future<CashReceipt>();

			var sourceCodesQuery = uow.Session.QueryOver(() => waterSourceCodeAlias)
				.JoinEntityAlias(() => productCodeAlias, () => waterSourceCodeAlias.Id == productCodeAlias.ResultCode.Id, JoinType.LeftOuterJoin)
				.WhereRestrictionOn(() => productCodeAlias.CashReceipt.Id).IsIn(receiptIdsArray)
				.Future<TrueMarkWaterIdentificationCode>();

			var productCodesQuery = uow.Session.QueryOver(() => productCodeAlias)
				.WhereRestrictionOn(() => productCodeAlias.CashReceipt.Id).IsIn(receiptIdsArray)
				.Future<CashReceiptProductCode>();

			var deliveryPointQuery = uow.Session.QueryOver(() => deliveryPointAlias)
				.JoinEntityAlias(() => orderAlias, () => deliveryPointAlias.Id == orderAlias.DeliveryPoint.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => receiptAlias, () => orderAlias.Id == receiptAlias.Order.Id, JoinType.LeftOuterJoin)
				.Where(receiptRestriction)
				.Fetch(SelectMode.Fetch, () => deliveryPointAlias.Category)
				.Future<DeliveryPoint>();

			var counterpartyQuery = uow.Session.QueryOver(() => counterpartyAlias)
				.JoinEntityAlias(() => orderAlias, () => counterpartyAlias.Id == orderAlias.Client.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => receiptAlias, () => orderAlias.Id == receiptAlias.Order.Id, JoinType.LeftOuterJoin)
				.Where(receiptRestriction)
				.Future<Counterparty>();

			var organizationQuery = uow.Session.QueryOver(() => organizationAlias)
				.JoinEntityAlias(() => contractAlias, () => organizationAlias.Id == contractAlias.Organization.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => orderAlias, () => contractAlias.Id == orderAlias.Contract.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => receiptAlias, () => orderAlias.Id == receiptAlias.Order.Id, JoinType.LeftOuterJoin)
				.Where(receiptRestriction)
				.Future<Organization>();

			var ordersQuery = uow.Session.QueryOver(() => orderAlias)
				.JoinEntityAlias(() => receiptAlias, () => orderAlias.Id == receiptAlias.Order.Id, JoinType.LeftOuterJoin)
				.Where(receiptRestriction)
				.Fetch(SelectMode.Fetch, () => orderAlias.Client)
				.Fetch(SelectMode.Fetch, () => orderAlias.Contract)
				.Future<VodovozOrder>();

			var result = receiptQuery.Distinct().ToList();
			return result;
		}

		public bool HasReceiptBySum(DateTime date, decimal sum)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var completedStatuses = new[] { FiscalDocumentStatus.Completed, FiscalDocumentStatus.WaitForCallback, FiscalDocumentStatus.Printed };
				var query = uow.Session.QueryOver(() => _cashReceiptAlias)
					.Where(
						Restrictions.Eq(
							Projections.SqlFunction(
								"DATE",
								NHibernateUtil.DateTime,
								Projections.Property(() => _cashReceiptAlias.CreateDate)
							),
							date.Date
						)
					)
					.Where(() => _cashReceiptAlias.Sum == sum)
					.WhereRestrictionOn(() => _cashReceiptAlias.FiscalDocumentStatus).IsIn(completedStatuses)
					.Select(Projections.Id());
				var result = query.List<int>();
				return result.Any();
			}
		}

		public bool HasReceipt(int orderId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var query = uow.Session.QueryOver(() => _cashReceiptAlias)
					.Where(() => _cashReceiptAlias.Order.Id == orderId)
					.Where(() => _cashReceiptAlias.Status == CashReceiptStatus.Sended)
					.Select(Projections.Id());
				var result = query.List<int>();
				return result.Any();
			}
		}

		public int GetCodeErrorsReceiptCount(IUnitOfWork uow)
		{
			var result = uow.Session.QueryOver<CashReceipt>()
				.Where(x => x.Status == CashReceiptStatus.CodeError)
				.ToRowCountQuery()
				.List<int>().First();
			return result;
		}

		public IEnumerable<int> GetReceiptIdsForPrepare(int count)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var statusesForPrepare = new[] { CashReceiptStatus.New, CashReceiptStatus.CodeError };
				CashReceipt cashReceiptAlias = null;
				var result = uow.Session.QueryOver(() => cashReceiptAlias)
					.WhereRestrictionOn(() => cashReceiptAlias.Status).IsIn(statusesForPrepare)
					.Select(Projections.Id())
					.OrderBy(() => cashReceiptAlias.Status).Asc
					.Take(count)
					.List<int>();
				return result;
			}
		}

		public IEnumerable<int> GetUnfinishedReceiptIds(int count)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var minTime = DateTime.Now.AddMinutes(-10);
				var maxTime = DateTime.Now.AddMonths(-1);
				var unfinishedStatuses = new[] {
					FiscalDocumentStatus.None,
					FiscalDocumentStatus.Queued,
					FiscalDocumentStatus.Pending,
					FiscalDocumentStatus.Printed,
					FiscalDocumentStatus.WaitForCallback
				};

				CashReceipt cashReceiptAlias = null;
				var result = uow.Session.QueryOver(() => cashReceiptAlias)
					.Where(() => cashReceiptAlias.Status == CashReceiptStatus.Sended)
					.Where(() => cashReceiptAlias.CreateDate <= minTime)
					.Where(() => cashReceiptAlias.CreateDate >= maxTime)
					.WhereRestrictionOn(() => cashReceiptAlias.FiscalDocumentStatus).IsIn(unfinishedStatuses)
					.Select(Projections.Id())
					.Take(count)
					.List<int>();
				return result;
			}
		}
		
		public int GetCashReceiptsCountForOrder(IUnitOfWork uow, int orderId)
		{
			var result = uow.Session.QueryOver<CashReceipt>()
				.Where(x => x.Order.Id == orderId)
				.Select(Projections.Count(Projections.Id()))
				.SingleOrDefault<int>();
			
			return result;
		}
	}
}
