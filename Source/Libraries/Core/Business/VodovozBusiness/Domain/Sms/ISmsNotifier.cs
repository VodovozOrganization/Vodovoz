using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Sms
{
	public interface ISmsNotifier
	{
		void NotifyIfNewClient(Order order);
		void NotifyUndeliveryAutoTransferNotApproved(UndeliveredOrder undeliveredOrder, IUnitOfWork externalUow = null);
	}
}
