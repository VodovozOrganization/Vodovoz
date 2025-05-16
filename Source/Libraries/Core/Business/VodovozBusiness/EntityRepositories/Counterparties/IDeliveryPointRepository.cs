using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Counterparties
{
	/// <summary>
	/// Интерфейс по запросам для точки доставки
	/// </summary>
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
		/// <summary>
		/// Получение координат точки доставки из онлайн заказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="externalOnlineOrderId">Id онлайн заказа в ИПЗ</param>
		/// <returns>Координаты ТД</returns>
		PointCoordinates DeliveryPointCoordinatesFromOnlineOrder(IUnitOfWork uow, Guid externalOnlineOrderId);
	}
}
