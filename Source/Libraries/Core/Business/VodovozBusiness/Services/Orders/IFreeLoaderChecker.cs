using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Nodes;

namespace VodovozBusiness.Services.Orders
{
	/// <summary>
	/// Проверки на возможных халявщиков
	/// </summary>
	public interface IFreeLoaderChecker
	{
		/// <summary>
		/// Найденные заказы с промиками по похожим адресам
		/// </summary>
		IEnumerable<FreeLoaderInfoNode> PossibleFreeLoadersByAddress { get; }
		/// <summary>
		/// Найденные заказы с промиками на телефон(ы)
		/// </summary>
		IEnumerable<FreeLoaderInfoNode> PossibleFreeLoadersByPhones { get; }

		/// <summary>
		/// Проверка на возможного халявщика.
		/// Идет поиск возможных соответствий по адресу
		/// также проверяются соответствия по телефонам, т.е. заказы промиков на этот номер телефона по другим заказам
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="orderId">Номер текущего заказа</param>
		/// <param name="deliveryPoint">Информация об адресе</param>
		/// <param name="phoneNumbers">Телефоны для проверки в формате XXXXXXXXXX(только цифры)</param>
		/// <param name="promoSetForNewClients">Промо набор для новых клиентов</param>
		/// <returns></returns>
		bool CheckFreeLoaders(
			IUnitOfWork uow,
			int orderId,
			DeliveryPoint deliveryPoint,
			IEnumerable<string> phoneNumbers,
			bool? promoSetForNewClients = null);
		/// <summary>
		/// Проверка на использование промонабора в заказе на адрес
		/// Если клиент физик заказывает на адрес с типом Склад <see cref="RoomType.Store"/> или Офис <see cref="RoomType.Office"/>
		/// </summary>
		/// /// <param name="uow">unit of work</param>
		/// <param name="isSelfDelivery">Самовывоз или нет</param>
		/// <param name="deliveryPoint">Информация об адресе</param>
		/// <param name="client">Информация о клиенте</param>
		/// <returns><c>true</c>, если на адрес доставляли промонабор для новых клиентов,
		/// <c>false</c> если нет</returns>
		bool CheckFreeLoaderOrderByNaturalClientToOfficeOrStore(
			IUnitOfWork uow,
			bool isSelfDelivery,
			Counterparty client,
			DeliveryPoint deliveryPoint);

		/// <summary>
		/// Проверка на возможного халявщика для ИПЗ(можно ли заказывать промик для нового клиента).
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="isSelfDelivery">Самовывоз или нет</param>
		/// <param name="counterpartyId">Id клиента</param>
		/// <param name="deliveryPointId">Id точки доставки</param>
		/// <param name="digitsNumber">Номер телефона в формате XXXXXXXXXX (только цифры)</param>
		/// <returns>
		/// <see cref="Result.IsSuccess"/> - может заказывать промик для новых клиентов,
		/// <see cref="Result.Failure(Error)"/> - нет</returns>
		Result CanOrderPromoSetForNewClientsFromOnline(
			IUnitOfWork uow,
			bool isSelfDelivery,
			int? counterpartyId,
			int? deliveryPointId,
			string digitsNumber = null);
	}
}
