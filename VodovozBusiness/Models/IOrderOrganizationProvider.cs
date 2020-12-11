using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Models
{
    public interface IOrderOrganizationProvider
    {
        Organization GetOrganization(IUnitOfWork uow, Order order);
    }
}