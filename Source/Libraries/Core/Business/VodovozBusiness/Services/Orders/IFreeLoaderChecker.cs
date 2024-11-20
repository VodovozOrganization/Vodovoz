using System.Collections.Generic;
using QS.DomainModel.UoW;
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
		/// <param name="phones">Телефоны для проверки</param>
		/// <returns></returns>
		bool CheckFreeLoaders(
			IUnitOfWork uow,
			int orderId,
			DeliveryPoint deliveryPoint,
			IEnumerable<Phone> phones);
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
	}
}
