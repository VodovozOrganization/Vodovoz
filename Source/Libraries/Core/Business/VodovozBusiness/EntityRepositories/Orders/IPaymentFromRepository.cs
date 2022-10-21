using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public interface IPaymentFromRepository
	{
		PaymentFrom GetDuplicatePaymentFromByName(IUnitOfWork uow, int id, string name);
	}
}
