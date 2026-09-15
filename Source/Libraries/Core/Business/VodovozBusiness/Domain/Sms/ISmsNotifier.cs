using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Sms
{
	public interface ISmsNotifier
	{
		/// <summary>
		/// Создаёт и сохраняет уведомление о первом заказе воды 19 л в отдельной единице работы.
		/// </summary>
		/// <param name="order">Подтверждаемый заказ.</param>
		void NotifyIfNewClient(Order order);
		void NotifyUndeliveryAutoTransferNotApproved(UndeliveredOrder undeliveredOrder, IUnitOfWork externalUow = null);
	}
}
