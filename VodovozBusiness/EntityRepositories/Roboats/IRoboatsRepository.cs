using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Roboats;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Roboats
{
	public interface IRoboatsRepository
	{
		IEnumerable<IRoboatsEntity> GetExportedEntities(RoboatsEntityType roboatsEntityType);
		int? GetBottlesReturnForOrder(int counterpartyId, int orderId);
		IEnumerable<int> GetCounterpartyIdsByPhone(string phone);
		string GetDeliveryPointApartment(int deliveryPointId, int counterpartyId);
		string GetDeliveryPointBuilding(int deliveryPointId, int counterpartyId);
		DeliverySchedule GetDeliverySchedule(int roboatsTimeId);
		Order GetLastOrder(int clientId, int? deliveryPointId = null);
		IEnumerable<Order> GetLastOrders(int clientId);
		IEnumerable<DeliverySchedule> GetRoboatsAvailableDeliveryIntervals();
		int GetRoboatsCounterpartyNameId(int counterpartyId, string phone);
		bool CounterpartyExcluded(int counterpartyId);
		int GetRoboatsCounterpartyPatronymicId(int counterpartyId, string phone);
		HashSet<Guid> GetAvailableForRoboatsFiasStreetGuids();
		int? GetRoboAtsStreetId(int counterPartyId, int deliveryPointId);
		IEnumerable<NomenclatureQuantity> GetWatersQuantityFromOrder(int counterpartyId, int orderId);
		IEnumerable<RoboatsWaterType> GetWaterTypes();
	}
}
