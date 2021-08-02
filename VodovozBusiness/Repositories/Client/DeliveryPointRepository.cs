using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repository.Client
{
	public static class DeliveryPointRepository
	{
		public static decimal GetAvgBottlesOrdered(IUnitOfWork uow, DeliveryPoint deliveryPoint, int? countLastOrders)
		{
			var deliveryPointrepository = new EntityRepositories.Delivery.DeliveryPointRepository();
			return deliveryPointrepository.GetAvgBottlesOrdered(uow, deliveryPoint, countLastOrders);
		}

		/// <summary>
		/// Запрос ищет точку доставки в контрагенте по коду 1с или целиком по адресной строке.
		/// </summary>
		public static DeliveryPoint GetByAddress1c(IUnitOfWork uow, Counterparty counterparty, string address1cCode, string address1c)
		{
			var deliveryPointrepository = new EntityRepositories.Delivery.DeliveryPointRepository();
			return deliveryPointrepository.GetByAddress1c(uow, counterparty, address1cCode, address1c);
		}

		public static IList<DeliveryPoint> GetDeliveryPointForCounterpartyByCoordinates(IUnitOfWork uow, decimal latitude, decimal longitude, int counterpartyId)
		{
			var deliveryPointrepository = new EntityRepositories.Delivery.DeliveryPointRepository();
			return deliveryPointrepository.GetDeliveryPointForCounterpartyByCoordinates(uow, latitude, longitude, counterpartyId);
		}
	}
}

