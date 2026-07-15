using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Type;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using VodovozBusiness.EntityRepositories.Edo;
using VodovozBusiness.Nodes;

namespace Vodovoz.Infrastructure.Persistance.Edo
{
	public class EdoDocflowRepository : IEdoDocflowRepository
	{
		public IList<EdoDockflowData> GetEdoDocflowDataByOrderId(IUnitOfWork uow, int orderId)
		{
			var data = (
				from orderEdoRequest in uow.Session.Query<FormalEdoRequest>()
				join documentEdoTask in uow.Session.Query<DocumentEdoTask>() on orderEdoRequest.Task.Id equals documentEdoTask.Id
				join oed in uow.Session.Query<OrderEdoDocument>() on documentEdoTask.Id equals oed.DocumentTaskId into orderEdoDocuments
				from orderEdoDocument in orderEdoDocuments.DefaultIfEmpty()
				join td in uow.Session.Query<TaxcomDocflow>() on orderEdoDocument.Id equals td.EdoDocumentId into taxcomDocflows
				from taxcomDocflow in taxcomDocflows.DefaultIfEmpty()
				join tda in uow.Session.Query<TaxcomDocflowAction>() on taxcomDocflow.Id equals tda.TaxcomDocflowId into taxcomDocflowActions
				from taxcomDocflowAction in taxcomDocflowActions.DefaultIfEmpty()

				let lastTaxcomDocflowActionTime = (DateTime?)uow.Session.Query<TaxcomDocflowAction>()
					.Where(x => x.TaxcomDocflowId == taxcomDocflow.Id)
					.OrderByDescending(x => x.Id)
					.Select(x => x.Time)
					.FirstOrDefault()

				where
					orderEdoRequest.Order.Id == orderId
					&& (taxcomDocflowAction == null || taxcomDocflowAction.Time == lastTaxcomDocflowActionTime)

				select new EdoDockflowData
				{
					OrderId = orderEdoRequest.Order.Id,
					DocFlowId = taxcomDocflow == null ? default : taxcomDocflow.DocflowId,
					EdoRequestCreationTime = orderEdoRequest.Time,
					TaxcomDocflowCreationTime = taxcomDocflow == null ? default(DateTime?) : taxcomDocflow.CreationTime,
					EdoDocFlowStatus = taxcomDocflowAction == null ? default(EdoDocFlowStatus?) : taxcomDocflowAction.DocFlowState,
					IsReceived = taxcomDocflow != null && taxcomDocflow.IsReceived,
					ErrorDescription = taxcomDocflowAction == null ? default : taxcomDocflowAction.ErrorMessage,
					IsNewDockflow = true,
					EdoDocumentType = orderEdoDocument == null ? default(EdoDocumentType?) : orderEdoDocument.DocumentType,
					EdoTaskStatus = documentEdoTask == null ? default(EdoTaskStatus?) : documentEdoTask.Status,
					EdoDocumentStatus = orderEdoDocument == null ? default(EdoDocumentStatus?) : orderEdoDocument.Status
				}
			).ToList();

			var informalDocumentsData = (
				from edoRequest in uow.Session.Query<InformalEdoRequest>()
				join edoTask in uow.Session.Query<OrderDocumentEdoTask>() on edoRequest.Task.Id equals edoTask.Id
				join oid in uow.Session.Query<OutgoingInformalEdoDocument>() on edoTask.Id equals oid.InformalDocumentTaskId into informalEdoDocuments
				from informalEdoDocument in informalEdoDocuments.DefaultIfEmpty()
				join td in uow.Session.Query<TaxcomDocflow>() on informalEdoDocument.Id equals td.EdoDocumentId into taxcomDocflows
				from taxcomDocflow in taxcomDocflows.DefaultIfEmpty()
				join tda in uow.Session.Query<TaxcomDocflowAction>() on taxcomDocflow.Id equals tda.TaxcomDocflowId into taxcomDocflowActions
				from taxcomDocflowAction in taxcomDocflowActions.DefaultIfEmpty()

				let lastTaxcomDocflowActionTime = (DateTime?)uow.Session.Query<TaxcomDocflowAction>()
					.Where(x => x.TaxcomDocflowId == taxcomDocflow.Id)
					.OrderByDescending(x => x.Id)
					.Select(x => x.Time)
					.FirstOrDefault()

				where
					edoRequest.Order.Id == orderId
					&& (taxcomDocflowAction == null || taxcomDocflowAction.Time == lastTaxcomDocflowActionTime)

				select new EdoDockflowData
				{
					OrderId = edoRequest.Order.Id,
					DocFlowId = taxcomDocflow == null ? default : taxcomDocflow.DocflowId,
					EdoRequestCreationTime = edoRequest.Time,
					TaxcomDocflowCreationTime = taxcomDocflow == null ? default(DateTime?) : taxcomDocflow.CreationTime,
					EdoDocFlowStatus = taxcomDocflowAction == null ? default(EdoDocFlowStatus?) : taxcomDocflowAction.DocFlowState,
					IsReceived = taxcomDocflow != null && taxcomDocflow.IsReceived,
					ErrorDescription = taxcomDocflowAction == null ? default : taxcomDocflowAction.ErrorMessage,
					IsNewDockflow = true,
					EdoDocumentType = EdoDocumentType.InformalOrderDocument,
					EdoTaskStatus = edoTask == null ? default(EdoTaskStatus?) : edoTask.Status,
					EdoDocumentStatus = informalEdoDocument == null ? default(EdoDocumentStatus?) : informalEdoDocument.Status,
					OrderDocumentType = edoRequest == null ? default(OrderDocumentType?) : edoRequest.OrderDocumentType
				}
			).ToList();

			return data.Concat(informalDocumentsData).ToList();
		}

		public IList<EdoDockflowData> GetEdoDocflowDataByClientId(IUnitOfWork uow, int clientId, int? maxResults = null)
		{
			var formalDocumentsData = GetFormalEdoDocflowDataByClientId(uow, clientId, maxResults);
			var informalDocumentsData = GetInformalEdoDocflowDataByClientId(uow, clientId, maxResults);

			return formalDocumentsData.Concat(informalDocumentsData).ToList();
		}

		private IList<EdoDockflowData> GetFormalEdoDocflowDataByClientId(IUnitOfWork uow, int clientId, int? maxResults = null)
		{
			var maxResultsClause = maxResults.HasValue ? "limit :max_results" : string.Empty;
			var sql = $@"
select
	ecr.order_id as :order_id,
	td.docflow_id as :docflow_id,
	ecr.`time` as :edo_request_creation_time,
	td.creation_time as :taxcom_docflow_creation_time,
	tda.state as :edo_docflow_status,
	if(td.id is null, 0, td.is_received) as :is_received,
	tda.error_message as :error_description,
	1 as :is_new_dockflow,
	eod.document_type as :edo_document_type,
	et.status as :edo_task_status,
	eod.status as :edo_document_status,
	null as :order_document_type
from (
	select
		ecr.order_id,
		ecr.order_task_id,
		ecr.`time`
	from edo_customer_requests ecr
	join orders o on o.id = ecr.order_id
	where o.client_id = :client_id
	order by ecr.`time` desc
	{maxResultsClause}
) ecr
join edo_tasks et on et.id = ecr.order_task_id and et.`type` = 'Document'
left join edo_outgoing_documents eod on eod.document_task_id = et.id and eod.`type` = 'Order'
left join taxcom_docflows td on td.edo_document_id = eod.id
left join taxcom_docflow_actions tda on tda.id = (
	select tda2.id
	from taxcom_docflow_actions tda2
	where tda2.taxcom_docflow_id = td.id
	order by tda2.id desc
	limit 1
)
order by ecr.`time` desc
";

			var query = CreateEdoDockflowDataQuery(uow, sql, clientId, maxResults);

			return query.List<EdoDockflowData>();
		}

		private IList<EdoDockflowData> GetInformalEdoDocflowDataByClientId(IUnitOfWork uow, int clientId, int? maxResults = null)
		{
			var maxResultsClause = maxResults.HasValue ? "limit :max_results" : string.Empty;
			var sql = $@"
select
	eir.order_id as :order_id,
	td.docflow_id as :docflow_id,
	eir.`time` as :edo_request_creation_time,
	td.creation_time as :taxcom_docflow_creation_time,
	tda.state as :edo_docflow_status,
	if(td.id is null, 0, td.is_received) as :is_received,
	tda.error_message as :error_description,
	1 as :is_new_dockflow,
	eod.document_type as :edo_document_type,
	et.status as :edo_task_status,
	eod.status as :edo_document_status,
	eir.order_document_type as :order_document_type
from (
	select
		eir.order_id,
		eir.order_document_task_id,
		eir.order_document_type,
		eir.`time`
	from edo_informal_requests eir
	join orders o on o.id = eir.order_id
	where o.client_id = :client_id
	order by eir.`time` desc
	{maxResultsClause}
) eir
join edo_tasks et on et.id = eir.order_document_task_id and et.`type` = 'InformalOrderDocument'
left join edo_outgoing_documents eod on eod.informal_document_task_id = et.id and eod.`type` = 'InformalOrderDocument'
left join taxcom_docflows td on td.edo_document_id = eod.id
left join taxcom_docflow_actions tda on tda.id = (
	select tda2.id
	from taxcom_docflow_actions tda2
	where tda2.taxcom_docflow_id = td.id
	order by tda2.id desc
	limit 1
)
order by eir.`time` desc
";

			var query = CreateEdoDockflowDataQuery(uow, sql, clientId, maxResults);

			return query.List<EdoDockflowData>();
		}

		private IQuery CreateEdoDockflowDataQuery(IUnitOfWork uow, string sql, int clientId, int? maxResults)
		{
			var query = uow.Session.CreateSQLQuery(sql)
				.MapParametersToNode<EdoDockflowData>()
				.Map("order_id", x => x.OrderId, NHibernateUtil.Int32)
				.Map("docflow_id", x => x.DocFlowId, NHibernateUtil.Guid)
				.Map("edo_request_creation_time", x => x.EdoRequestCreationTime, NHibernateUtil.DateTime)
				.Map("taxcom_docflow_creation_time", x => x.TaxcomDocflowCreationTime, NHibernateUtil.DateTime)
				.Map("edo_docflow_status", x => x.EdoDocFlowStatus, new EnumStringType<EdoDocFlowStatus>())
				.Map("is_received", x => x.IsReceived, NHibernateUtil.Boolean)
				.Map("error_description", x => x.ErrorDescription, NHibernateUtil.String)
				.Map("is_new_dockflow", x => x.IsNewDockflow, NHibernateUtil.Boolean)
				.Map("edo_document_type", x => x.EdoDocumentType, new EnumStringType<EdoDocumentType>())
				.Map("edo_task_status", x => x.EdoTaskStatus, new EnumStringType<EdoTaskStatus>())
				.Map("edo_document_status", x => x.EdoDocumentStatus, new EnumStringType<EdoDocumentStatus>())
				.Map("order_document_type", x => x.OrderDocumentType, new EnumStringType<OrderDocumentType>())
				.SetResultTransformer()
				.SetParameter("client_id", clientId);

			if(maxResults.HasValue)
			{
				query.SetParameter("max_results", maxResults.Value);
				query.SetMaxResults(maxResults.Value);
			}

			return query;
		}

		public IList<EdoDockflowData> GetOldEdoDocflowDataByClientId(IUnitOfWork uow, int clientId, int? maxResults = null)
		{
			var query =
				from edoContainer in uow.Session.Query<EdoContainer>()
				where edoContainer.Counterparty.Id == clientId
					&& !edoContainer.IsIncoming
				orderby edoContainer.Created descending
				select new EdoDockflowData
				{
					OrderId = edoContainer.Order == null ? default(int?) : edoContainer.Order.Id,
					DocFlowId = edoContainer.DocFlowId,
					OldEdoDocumentType = edoContainer.Type,
					EdoDocFlowStatus = edoContainer.EdoDocFlowStatus,
					IsReceived = edoContainer.Received,
					ErrorDescription = edoContainer.ErrorDescription,
					OrderWithoutShipmentForAdvancePaymentId = edoContainer.OrderWithoutShipmentForAdvancePayment == null
						? default(int?)
						: edoContainer.OrderWithoutShipmentForAdvancePayment.Id,
					OrderWithoutShipmentForDebtId = edoContainer.OrderWithoutShipmentForDebt == null
						? default(int?)
						: edoContainer.OrderWithoutShipmentForDebt.Id,
					OrderWithoutShipmentForPaymentId = edoContainer.OrderWithoutShipmentForPayment == null
						? default(int?)
						: edoContainer.OrderWithoutShipmentForPayment.Id,
					TaxcomDocflowCreationTime = edoContainer.Created,
					IsNewDockflow = false
				};

			if(maxResults.HasValue)
			{
				query = query.Take(maxResults.Value);
			}

			return query.ToList();
		}

		/// <inheritdoc/>
		public async Task<IEnumerable<int>> GetNotProcessedDriversScannedCodesRouteListAddressIds(
			IUnitOfWork uow,
			CancellationToken cancellationToken)
		{
			var query =
				from driversScannedCode in uow.Session.Query<DriversScannedTrueMarkCode>()
				where
					driversScannedCode.DriversScannedTrueMarkCodeStatus == DriversScannedTrueMarkCodeStatus.None
					|| (driversScannedCode.DriversScannedTrueMarkCodeStatus == DriversScannedTrueMarkCodeStatus.Error
						&& driversScannedCode.DriversScannedTrueMarkCodeError == DriversScannedTrueMarkCodeError.TrueMarkApiRequestError)
				select driversScannedCode.RouteListAddressId;

			return await query.Distinct().ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IEnumerable<DriversScannedCodeDataNode>> GetNotProcessedDriversScannedCodesDataByRouteListItemId(
			IUnitOfWork uow,
			int routeListItemId,
			CancellationToken cancellationToken)
		{
			var query =
				from driversScannedCode in uow.Session.Query<DriversScannedTrueMarkCode>()
				join orderItem in uow.Session.Query<OrderItemEntity>() on driversScannedCode.OrderItemId equals orderItem.Id
				where
					(driversScannedCode.DriversScannedTrueMarkCodeStatus == DriversScannedTrueMarkCodeStatus.None
					|| (driversScannedCode.DriversScannedTrueMarkCodeStatus == DriversScannedTrueMarkCodeStatus.Error
						&& driversScannedCode.DriversScannedTrueMarkCodeError == DriversScannedTrueMarkCodeError.TrueMarkApiRequestError))
					&& driversScannedCode.RouteListAddressId == routeListItemId
				select new DriversScannedCodeDataNode
				{
					DriversScannedCode = driversScannedCode,
					OrderItem = orderItem
				};

			return await query.ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IEnumerable<FormalEdoRequest>> GetOrderEdoRequestsByOrderId(IUnitOfWork uow, int orderId, CancellationToken cancellationToken)
		{
			var orderEdoRequests = await uow.Session.Query<FormalEdoRequest>()
				.Where(x => x.Order.Id == orderId)
				.ToListAsync(cancellationToken);

			return orderEdoRequests;
		}

		public TaxcomDocflow GetLastTaxcomDocflowByOrderId(IUnitOfWork uow, int orderId)
		{
			TaxcomDocflow taxcomDocflowAlias = null;
			OrderEdoDocument outgoingEdoDocumentAlias = null;
			EdoTask edoTaskAlias = null;
			FormalEdoRequest formalEdoRequestAlias = null;

			var taxcomDocflow = uow.Session.QueryOver(() => taxcomDocflowAlias)
				.JoinEntityAlias(() => outgoingEdoDocumentAlias, () => outgoingEdoDocumentAlias.Id == taxcomDocflowAlias.EdoDocumentId)
				.JoinEntityAlias(() => edoTaskAlias, () => edoTaskAlias.Id == outgoingEdoDocumentAlias.DocumentTaskId)
				.JoinEntityAlias(() => formalEdoRequestAlias, () => formalEdoRequestAlias.Task.Id == edoTaskAlias.Id)
				.Where(() => formalEdoRequestAlias.Order.Id == orderId)
				.OrderByAlias(() => taxcomDocflowAlias.CreationTime).Desc
				.Take(1)
				.SingleOrDefault();

			return taxcomDocflow;
		}
	}
}
