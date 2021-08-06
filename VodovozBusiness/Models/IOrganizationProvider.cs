using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Models
{
	public interface IOrganizationProvider
	{
		Organization GetOrganization(IUnitOfWork uow, Order order);

		Organization GetOrganization(IUnitOfWork uow, PaymentType paymentType, bool isSelfDelivery,
			IEnumerable<OrderItem> orderItems = null, PaymentFrom paymentFrom = null, GeographicGroup geographicGroup = null);

		Organization GetOrganizationForOrderWithoutShipment(IUnitOfWork uow, OrderWithoutShipmentForAdvancePayment order);
	}
}
