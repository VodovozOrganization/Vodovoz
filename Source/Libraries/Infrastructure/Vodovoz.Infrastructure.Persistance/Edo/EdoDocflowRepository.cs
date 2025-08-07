using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using VodovozBusiness.EntityRepositories.Edo;
using VodovozBusiness.Nodes;

namespace Vodovoz.Infrastructure.Persistance.Edo
{
	public class EdoDocflowRepository : IEdoDocflowRepository
	{
		public IList<EdoDockflowData> GetEdoDocflowDataByOrderId(IUnitOfWork uow, int orderId)
		{
			var data =
				from orderEdoRequest in uow.Session.Query<OrderEdoRequest>()
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
					&& (taxcomDocflowAction.Id == null || taxcomDocflowAction.Time == lastTaxcomDocflowActionTime)

				select new EdoDockflowData
				{
					OrderId = orderEdoRequest.Order.Id,
					DocFlowId = taxcomDocflow == null ? default(Guid?) : taxcomDocflow.DocflowId,
					EdoRequestCreationTime = orderEdoRequest.Time,
					TaxcomDocflowCreationTime = taxcomDocflow == null ? default(DateTime?) : taxcomDocflow.CreationTime,
					EdoDocFlowStatus = taxcomDocflowAction == null ? default(EdoDocFlowStatus?) : taxcomDocflowAction.DocFlowState,
					IsReceived = taxcomDocflow != null && taxcomDocflow.IsReceived,
					ErrorDescription = taxcomDocflowAction == null ? default : taxcomDocflowAction.ErrorMessage,
					IsNewDockflow = true,
					EdoDocumentType = orderEdoDocument == null ? default(EdoDocumentType?) : orderEdoDocument.DocumentType,
					EdoTaskStatus = documentEdoTask == null ? default(EdoTaskStatus?) : documentEdoTask.Status,
					EdoDocumentStatus = orderEdoDocument == null ? default(EdoDocumentStatus?) : orderEdoDocument.Status
				};

			return data.ToList();
		}

		public IList<EdoDockflowData> GetEdoDocflowDataByClientId(IUnitOfWork uow, int clientId)
		{
			var data =
				from client in uow.Session.Query<Counterparty>()
				join order in uow.Session.Query<Order>() on client.Id equals order.Client.Id
				join orderEdoRequest in uow.Session.Query<OrderEdoRequest>() on order.Id equals orderEdoRequest.Order.Id
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
					client.Id == clientId
					&& (taxcomDocflowAction.Id == null || taxcomDocflowAction.Time == lastTaxcomDocflowActionTime)

				select new EdoDockflowData
				{
					OrderId = order.Id,
					DocFlowId = taxcomDocflow == null ? default(Guid?) : taxcomDocflow.DocflowId,
					EdoRequestCreationTime = orderEdoRequest.Time,
					TaxcomDocflowCreationTime = taxcomDocflow == null ? default(DateTime?) : taxcomDocflow.CreationTime,
					EdoDocFlowStatus = taxcomDocflowAction == null ? default(EdoDocFlowStatus?) : taxcomDocflowAction.DocFlowState,
					IsReceived = taxcomDocflow != null && taxcomDocflow.IsReceived,
					ErrorDescription = taxcomDocflowAction == null ? default : taxcomDocflowAction.ErrorMessage,
					IsNewDockflow = true,
					EdoDocumentType = orderEdoDocument == null ? default(EdoDocumentType?) : orderEdoDocument.DocumentType,
					EdoTaskStatus = documentEdoTask == null ? default(EdoTaskStatus?) : documentEdoTask.Status,
					EdoDocumentStatus = orderEdoDocument == null ? default(EdoDocumentStatus?) : orderEdoDocument.Status
				};

			return data.ToList();
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
		public async Task<IEnumerable<OrderEdoRequest>> GetOrderEdoRequestsByOrderId(IUnitOfWork uow, int orderId, CancellationToken cancellationToken)
		{
			var orderEdoRequests = await uow.Session.Query<OrderEdoRequest>()
				.Where(x => x.Order.Id == orderId)
				.ToListAsync(cancellationToken);

			return orderEdoRequests;
		}
	}
}
