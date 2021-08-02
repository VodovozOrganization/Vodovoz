using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Delivery
{
	public interface IDeliveryPointRepository
	{
		IList<DeliveryPoint> DeliveryPointsForCounterpartyQuery(IUnitOfWork uow, Counterparty counterparty);
		decimal GetAvgBottlesOrdered(IUnitOfWork uow, DeliveryPoint deliveryPoint, int? countLastOrders);
		//int GetBottlesOrderedForPeriod(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime start, DateTime end);
		DeliveryPoint GetByAddress1c(IUnitOfWork uow, Counterparty counterparty, string address1cCode, string address1c);
		IList<DeliveryPoint> GetDeliveryPointForCounterpartyByCoordinates(IUnitOfWork uow, decimal latitude, decimal longitude, int counterpartyId);
	}
}
