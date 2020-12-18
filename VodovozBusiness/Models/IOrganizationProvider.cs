using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Models
{
    public interface IOrganizationProvider
    {
        Organization GetOrganization(IUnitOfWork uow, Order order);
        
        Organization GetOrganizationForOrderWithoutShipment(IUnitOfWork uow, OrderWithoutShipmentForAdvancePayment order);
        
        int GetMainOrganization();
    }
}