using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Client;

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
					   join organization in uow.Session.Query<CounterpartyEntity>()
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
				.ToListAsync(cancellationToken);

			return orders;
		}
	}
}
