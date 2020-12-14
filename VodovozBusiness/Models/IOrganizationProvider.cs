using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Models
{
    public interface IOrganizationProvider
    {
        Organization GetOrganization(IUnitOfWork uow, Order order);
        
        int GetMainOrganization();
    }
}