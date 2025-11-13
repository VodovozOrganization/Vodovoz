using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Nodes;

namespace VodovozBusiness.EntityRepositories.Orders
{
	public interface IFreeLoaderRepository
	{
		/// <summary>
		/// Выборка информации о заказах с промонаборами, которые заказывались на такой же адрес
		/// сравнение по строковым значениям города, улицы, дома и квартиры
		/// Если найдется хоть одно совпадение, значит клиент может являться халявщиком
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="orderId">Номер текущего заказа для исключения из выборки</param>
		/// <param name="deliveryPoint">Точка доставки</param>
		/// <param name="promoSetForNewClients">Промо набор для новых клиентов</param>
		/// <returns>Набор данных о ранних заказах промиков на аналогичный адрес <see cref="FreeLoaderInfoNode"/></returns>
		IEnumerable<FreeLoaderInfoNode> GetPossibleFreeLoadersByAddress(
			IUnitOfWork uow,
			int orderId,
			DeliveryPoint deliveryPoint,
			bool? promoSetForNewClients = null);

		/// <summary>
		/// Выборка информации о заказах с промонаборами, которые заказывались на такой же телефон
		/// сравнение по номерам телефонов клиента
		/// Если найдется хоть одно совпадение, значит клиент может являться халявщиком
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="orderId">Номер текущего заказа для исключения из выборки</param>
		/// <param name="phones">Список номеров телефонов в формате XXXXXXXXXX(только цифры)</param>
		/// <returns>Набор данных о ранних заказах промиков по указанным телефонам <see cref="FreeLoaderInfoNode"/></returns>
		IEnumerable<FreeLoaderInfoNode> GetPossibleFreeLoadersInfoByCounterpartyPhones(
			IUnitOfWork uow,
			int orderId,
			IEnumerable<string> phones);

		/// <summary>
		/// Выборка информации о заказах с промонаборами, которые заказывались на такой же телефон
		/// сравнение по номерам телефонов точки доставки
		/// Если найдется хоть одно совпадение, значит клиент может являться халявщиком
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="excludeOrderIds">Номера исключаемых заказов</param>
		/// <param name="phones">Список номеров телефонов в формате XXXXXXXXXX(только цифры)</param>
		/// <returns>Набор данных о ранних заказах промиков по указанным телефонам <see cref="FreeLoaderInfoNode"/></returns>
		IEnumerable<FreeLoaderInfoNode> GetPossibleFreeLoadersInfoByDeliveryPointPhones(
			IUnitOfWork uow,
			IEnumerable<int> excludeOrderIds,
			IEnumerable<string> phones);
		/// <summary>
		/// Выборка информации о заказах с промонаборами, которые заказывались на такой же адрес(сравнение по Guid дома в ФИАСе) с квартирой
		/// Если найдется хоть одно совпадение, значит клиент может являться халявщиком
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="buildingFiasGuid">Guid дома в ФИАСе</param>
		/// <param name="room">Номер квартиры</param>
		/// <param name="orderId">Номер текущего заказа для исключения из выборки</param>
		/// <param name="promoSetForNewClients">Промик для новых клиентов или нет, по умолчанию true</param>
		/// <returns>Набор данных о ранних заказах промиков на аналогичный адрес <see cref="FreeLoaderInfoNode"/></returns>
		IEnumerable<FreeLoaderInfoNode> GetPossibleFreeLoadersInfoByBuildingFiasGuid(
			IUnitOfWork uow, Guid buildingFiasGuid, string room, int orderId, bool promoSetForNewClients = true);
		/// <summary>
		/// Проверка наличия не отмененных онлайн заказов с промо наборами для новых клиентов по ТД
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="deliveryPointId">Id точки доставки</param>
		/// <returns></returns>
		bool HasOnlineOrderWithPromoSetForNewClients(IUnitOfWork uow, int deliveryPointId);
	}
}
