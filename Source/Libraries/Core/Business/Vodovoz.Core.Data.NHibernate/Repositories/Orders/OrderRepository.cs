using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Orders
{
	public class OrderRepository : IOrderRepository
	{
		public async Task<IEnumerable<OrderEntity>> GetOrdersForEquipmentTransferEdoAsync(
			IUnitOfWork uow,
			int maxDaysFromDeliveryDate,
			int closingDocumentDeliveryScheduleId,
			IEnumerable<OrderStatus> orderStatusesToSendUpd,
			CancellationToken cancellationToken)
		{
			var orders =
				await (from order in uow.Session.Query<OrderEntity>()
					   join client in uow.Session.Query<CounterpartyEntity>() on order.Client.Id equals client.Id
					   join contract in uow.Session.Query<CounterpartyContractEntity>()
						   on order.Contract.Id equals contract.Id
					   join organization in uow.Session.Query<OrganizationEntity>()
						   on contract.Organization.Id equals organization.Id
					   join er in uow.Session.Query<EquipmentTransferEdoRequest>() on order.Id equals er.Order.Id into edoRequests
					   from edoRequest in edoRequests.DefaultIfEmpty()
					   join defaultEdoAccount in uow.Session.Query<CounterpartyEdoAccountEntity>()
						   on new { a = client.Id, b = (int?)contract.Organization.Id, c = true }
						   equals new { a = defaultEdoAccount.Counterparty.Id, b = defaultEdoAccount.OrganizationId, c = defaultEdoAccount.IsDefault }
						   into edoAccountsByOrder
					   from edoAccountByOrder in edoAccountsByOrder.DefaultIfEmpty()
					   where
						   order.PaymentType == PaymentType.Cashless
						   && order.DeliveryDate >= DateTime.Today.AddDays(-maxDaysFromDeliveryDate)
						   && order.DeliverySchedule.Id == closingDocumentDeliveryScheduleId
						   && orderStatusesToSendUpd.Contains(order.OrderStatus)
						   && client.IsNewEdoProcessing
						   && edoAccountByOrder.ConsentForEdoStatus == ConsentForEdoStatus.Agree
						   && !client.IsNotSendEquipmentTransferByEdo
						   && edoRequest == null
					   select order)
				.Distinct()
				.ToListAsync(cancellationToken);

			var ordersWithEquipmentTransfer = orders
				.Where(order => order.OrderDocuments.Any(doc => doc.Type == OrderDocumentType.EquipmentTransfer))
				.ToList();

			return ordersWithEquipmentTransfer;
		}

		public async Task<IEnumerable<OrderEntity>> GetOrdersForClosingDocumentUpdEdoAsync(
			IUnitOfWork uow,
			int maxDaysFromDeliveryDate,
			int closingDocumentDeliveryScheduleId,
			IEnumerable<OrderStatus> orderStatusesToSendUpd,
			CancellationToken cancellationToken)
		{
			var orders =
				await (from order in uow.Session.Query<OrderEntity>()
					   join client in uow.Session.Query<CounterpartyEntity>() on order.Client.Id equals client.Id
					   join contract in uow.Session.Query<CounterpartyContractEntity>()
						   on order.Contract.Id equals contract.Id
					   join organization in uow.Session.Query<OrganizationEntity>()
						   on contract.Organization.Id equals organization.Id
					   join er in uow.Session.Query<PrimaryEdoRequest>()
						   on new { OrderId = order.Id, DocumentType = EdoDocumentType.UPD }
						   equals new { OrderId = er.Order.Id, er.DocumentType }
						   into edoRequests
					   from edoRequest in edoRequests.DefaultIfEmpty()
					   join ec in uow.Session.Query<EdoContainerEntity>()
						   on new { OrderId = order.Id, DocumentType = DocumentContainerType.Upd }
						   equals new { OrderId = ec.Order.Id, DocumentType = ec.Type }
						   into edoContainers
					   from edoContainer in edoContainers.DefaultIfEmpty()
					   join defaultEdoAccount in uow.Session.Query<CounterpartyEdoAccountEntity>()
						   on new { a = client.Id, b = (int?)contract.Organization.Id, c = true }
						   equals new { a = defaultEdoAccount.Counterparty.Id, b = defaultEdoAccount.OrganizationId, c = defaultEdoAccount.IsDefault }
						   into edoAccountsByOrder
					   from edoAccountByOrder in edoAccountsByOrder.DefaultIfEmpty()
					   where
						   order.PaymentType == PaymentType.Cashless
						   && order.DeliveryDate >= DateTime.Today.AddDays(-maxDaysFromDeliveryDate)
						   && order.DeliverySchedule.Id == closingDocumentDeliveryScheduleId
						   && orderStatusesToSendUpd.Contains(order.OrderStatus)
						   && client.IsNewEdoProcessing
						   && edoAccountByOrder.ConsentForEdoStatus == ConsentForEdoStatus.Agree
						   && !client.IsNotSendDocumentsByEdo
						   && edoContainer == null
						   && edoRequest == null
					   select order)
				.Distinct()
				.ToListAsync(cancellationToken);

			return orders;
		}

		public async Task<IEnumerable<PrimaryEdoRequest>> GetUnprocessedClosingDocumentUpdEdoRequestsAsync(
			IUnitOfWork uow,
			int maxDaysFromDeliveryDate,
			int closingDocumentDeliveryScheduleId,
			IEnumerable<OrderStatus> orderStatusesToSendUpd,
			CancellationToken cancellationToken)
		{
			var edoRequests =
				await (from request in uow.Session.Query<PrimaryEdoRequest>()
					   join order in uow.Session.Query<OrderEntity>() on request.Order.Id equals order.Id
					   join client in uow.Session.Query<CounterpartyEntity>() on order.Client.Id equals client.Id
					   join contract in uow.Session.Query<CounterpartyContractEntity>()
						   on order.Contract.Id equals contract.Id
					   join organization in uow.Session.Query<OrganizationEntity>()
						   on contract.Organization.Id equals organization.Id
					   join defaultEdoAccount in uow.Session.Query<CounterpartyEdoAccountEntity>()
						   on new { a = client.Id, b = (int?)contract.Organization.Id, c = true }
						   equals new { a = defaultEdoAccount.Counterparty.Id, b = defaultEdoAccount.OrganizationId, c = defaultEdoAccount.IsDefault }
						   into edoAccountsByOrder
					   from edoAccountByOrder in edoAccountsByOrder.DefaultIfEmpty()
					   where
						   request.DocumentType == EdoDocumentType.UPD
						   && request.Task == null
						   && order.PaymentType == PaymentType.Cashless
						   && order.DeliveryDate >= DateTime.Today.AddDays(-maxDaysFromDeliveryDate)
						   && order.DeliverySchedule.Id == closingDocumentDeliveryScheduleId
						   && orderStatusesToSendUpd.Contains(order.OrderStatus)
						   && client.IsNewEdoProcessing
						   && edoAccountByOrder.ConsentForEdoStatus == ConsentForEdoStatus.Agree
						   && !client.IsNotSendDocumentsByEdo
					   select request)
				.Distinct()
				.ToListAsync(cancellationToken);

			return edoRequests;
		}
	}
}
