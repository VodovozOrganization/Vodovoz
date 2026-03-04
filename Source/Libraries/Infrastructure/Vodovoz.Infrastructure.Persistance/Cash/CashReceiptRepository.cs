using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Settings.Orders;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Cash
{
	internal sealed class CashReceiptRepository : ICashReceiptRepository
	{
		private OrderItem _orderItemAlias = null;
		private VodovozOrder _orderAlias = null;
		private CashReceipt _cashReceiptAlias = null;
		private Counterparty _counterpartyAlias = null;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderSettings _orderSettings;

		public CashReceiptRepository(IUnitOfWorkFactory uowFactory, IOrderSettings orderSettings)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
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
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderSettings.PaymentFromTerminalId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderSettings.GetPaymentByCardFromFastPaymentServiceId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderSettings.GetPaymentByCardFromAvangardId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderSettings.GetPaymentByCardFromSiteByQrCodeId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderSettings.GetPaymentByCardFromMobileAppByQrCodeId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderSettings.PaymentByCardFromMobileAppId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderSettings.PaymentByCardFromSiteId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderSettings.PaymentFromSmsYuKassaId)
						.Add(() => _orderAlias.PaymentByCardFrom.Id == _orderSettings.GetPaymentByCardFromKulerSaleId)
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
					.And(GetPaymentTypeRestriction())
					.And(GetSelfdeliveryRestriction())
					.And(GetPositiveSumRestriction())
					.And(GetDeliveryDateRestriction())
					.And(() => !_counterpartyAlias.IsNewEdoProcessing)
					;

				var result =
					query.SelectList(list => list
							.SelectGroup(() => _orderAlias.Id))
						.List<int>();
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
					.And(() => !_orderAlias.SelfDelivery)
					.And(() => !_counterpartyAlias.IsNewEdoProcessing);

				var result =
					query.SelectList(list => list
							.SelectGroup(() => _orderAlias.Id))
						.List<int>();
				return result;
			}
		}

		public IEnumerable<CashReceipt> GetCashReceiptsForSend(IUnitOfWork uow, int count)
		{
			var queryReady = uow.Session.QueryOver(() => _cashReceiptAlias)
				.Where(() => _cashReceiptAlias.Status == CashReceiptStatus.ReadyToSend)
				.Select(Projections.Id())
				.OrderBy(() => _cashReceiptAlias.CreateDate).Asc
				.Take(count);

			var queryError = uow.Session.QueryOver(() => _cashReceiptAlias)
				.Where(() => _cashReceiptAlias.Status == CashReceiptStatus.ReceiptSendError)
				.Select(Projections.Id())
				.OrderBy(() => _cashReceiptAlias.CreateDate).Asc
				.Take(count);

			var readyReceiptIds = queryReady.List<int>();
			IEnumerable<int> receiptIds = readyReceiptIds;

			if(readyReceiptIds.Count < count)
			{
				var receiptIdsWithError = queryError.List<int>();
				receiptIds = readyReceiptIds.Union(receiptIdsWithError);
			}

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
				//Эта проверка нужна для проверки смены договора
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

		public IEnumerable<CashReceipt> GetReceiptsForOrder(IUnitOfWork uow, int orderId, CashReceiptStatus? cashReceiptStatus = null)
		{
			var query =
				from receipt in uow.Session.Query<CashReceipt>()
				where receipt.Order.Id == orderId
				select receipt;

			if(cashReceiptStatus != null)
			{
				return query
					.Where(x => x.Status == cashReceiptStatus)
					.ToList();
			}

			return query.ToList();
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

		public bool HasNeededReceipt(int orderId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				if(HasNeededReceiptOldDocflow(uow, orderId))
				{
					return true;
				}

				return IsReceiptSentNewDocflow(uow, orderId);
			}
		}

		public bool HasNeededReceiptOldDocflow(IUnitOfWork uow, int orderId)
		{
			var query = uow.Session.QueryOver(() => _cashReceiptAlias)
					.Where(() => _cashReceiptAlias.Order.Id == orderId)
					.And(() => _cashReceiptAlias.Status != CashReceiptStatus.ReceiptNotNeeded
						&& _cashReceiptAlias.Status != CashReceiptStatus.DuplicateSum)
					.Select(Projections.Id());
			var result = query.List<int>();

			return result.Any();
		}

		private bool IsReceiptSentNewDocflow(IUnitOfWork uow, int orderId)
		{
			var fiscalDocumentStages = new[]
			{
				FiscalDocumentStage.Sent,
				FiscalDocumentStage.Completed
			};

			var receipts =
				(from edoTask in uow.Session.Query<ReceiptEdoTask>()
				 join edoRequest in uow.Session.Query<FormalEdoRequest>() on edoTask.Id equals edoRequest.Task.Id
				 join efd in uow.Session.Query<EdoFiscalDocument>() on edoTask.Id equals efd.ReceiptEdoTask.Id into fiscalDocuments
				 from fiscalDocument in fiscalDocuments.DefaultIfEmpty()
				 join tri in uow.Session.Query<TransferEdoRequestIteration>() on edoTask.Id equals tri.OrderEdoTask.Id into transferEdoRequestIterations
				 from transferEdoRequestIteration in transferEdoRequestIterations.DefaultIfEmpty()
				 join ter in uow.Session.Query<TransferEdoRequest>() on transferEdoRequestIteration.Id equals ter.Iteration.Id into transferEdoRequests
				 from transferEdoRequest in transferEdoRequests.DefaultIfEmpty()
				 join tet in uow.Session.Query<TransferEdoTask>() on transferEdoRequest.TransferEdoTask.Id equals tet.Id into transferEdoTasks
				 from transferEdoTask in transferEdoTasks.DefaultIfEmpty()
				 join ted in uow.Session.Query<TransferEdoDocument>() on transferEdoTask.Id equals ted.TransferTaskId into transferEdoDocuments
				 from transferEdoDocument in transferEdoDocuments.DefaultIfEmpty()
				 where
					 edoRequest.Order.Id == orderId
					 && (transferEdoDocument.Id != null || fiscalDocumentStages.Contains(fiscalDocument.Stage))
				 select
				 edoTask.Id)
				.ToList();

			return receipts.Any();
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

		public EdoFiscalDocument GetLastEdoFiscalDocumentByOrderId(IUnitOfWork uow, int orderId)
		{
			EdoFiscalDocument fiscalDocumentAlias = null;
			ReceiptEdoTask receiptEdoTaskAlias = null;
			FormalEdoRequest formalEdoRequestAlias = null;

			var fiscalDocument = uow.Session.QueryOver(() => fiscalDocumentAlias)
				.JoinAlias(() => fiscalDocumentAlias.ReceiptEdoTask, () => receiptEdoTaskAlias)
				.JoinAlias(() => receiptEdoTaskAlias.FormalEdoRequest, () => formalEdoRequestAlias)
				.Where(() => formalEdoRequestAlias.Order.Id == orderId)
				.OrderByAlias(() => fiscalDocumentAlias.CreationTime)
				.Desc
				.Take(1)
				.SingleOrDefault();

			return fiscalDocument;
		}
	}
}
