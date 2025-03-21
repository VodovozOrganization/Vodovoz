using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Services.Orders
{
	public interface IOrderContractUpdater
	{
		void UpdateContract(IUnitOfWork uow, Order order, bool onPaymentTypeChanged = false);
		void ForceUpdateContract(IUnitOfWork uow, Order order, Organization organization = null);

		void UpdateOrCreateContract(
			IUnitOfWork uow,
			Order order,
			Organization organization = null);
	}
}
