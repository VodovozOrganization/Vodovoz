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
		IEnumerable<int> GetLastDeliveryPointIds(int clientId);
		Order GetLastOrder(int counterpartyId, int? deliveryPoint = null);
		IEnumerable<DeliverySchedule> GetRoboatsAvailableDeliveryIntervals();
		int GetRoboatsCounterpartyNameId(int counterpartyId, string phone);
		int GetRoboatsCounterpartyPatronymicId(int counterpartyId, string phone);
		int? GetRoboAtsStreetId(int counterPartyId, int deliveryPointId);
		IEnumerable<NomenclatureQuantity> GetWatersQuantityFromOrder(int counterpartyId, int orderId);
		IEnumerable<RoboatsWaterType> GetWaterTypes();
	}
}
