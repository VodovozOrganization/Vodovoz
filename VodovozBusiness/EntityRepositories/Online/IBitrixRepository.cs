using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories 
{
    public interface IBitrixRepository
    {
        BitrixDealRegistration GetDealRegistration(IUnitOfWork uow, uint bitrixDealId);
        BitrixDealRegistration GetDealRegistrationByOrder(IUnitOfWork uow, int orderId);
	}
}