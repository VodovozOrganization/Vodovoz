using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface IDeliveryPointRepository
	{
		/// <summary>
		/// Запрос ищет точку доставки в контрагенте по коду 1с или целиком по адресной строке.
		/// </summary>
		DeliveryPoint GetByAddress1c(IUnitOfWork uow, Domain.Client.Counterparty counterparty, string address1cCode, string address1c);

		int GetBottlesOrderedForPeriod(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime start, DateTime end);
		decimal GetAvgBottlesOrdered(IUnitOfWork uow, DeliveryPoint deliveryPoint, int? countLastOrders);
		/// <summary>
		/// Возвращает активные категории точек доставки, упорядоченные по имени
		/// </summary>
		/// <param name="uow"></param>
		/// <returns></returns>
		IOrderedEnumerable<DeliveryPointCategory> GetActiveDeliveryPointCategories(IUnitOfWork uow);

		IList<DeliveryPoint> GetDeliveryPointsByCounterpartyId(IUnitOfWork uow, int counterpartyId);

		IEnumerable<string> GetAddressesWithFixedPrices(int counterpartyId);

		bool CheckingAnAddressForDeliveryForNewCustomers( IUnitOfWork uow, DeliveryPoint deliveryPoint );
		IEnumerable<DeliveryPointForSendNode> GetActiveDeliveryPointsForSendByCounterpartyId(IUnitOfWork uow, int counterpartyId);
		bool ClientDeliveryPointExists(IUnitOfWork uow, int counterpartyId, int deliveryPointId);
	}
}
