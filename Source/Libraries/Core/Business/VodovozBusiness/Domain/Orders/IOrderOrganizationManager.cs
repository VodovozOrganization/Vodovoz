using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;

namespace VodovozBusiness.Domain.Orders
{
	/// <summary>
	/// Контракт обработчика для подбора организаций для заказа
	/// </summary>
	public interface IOrderOrganizationManager : ISplitOrderByOrganizations
	{
		/// <summary>
		/// Получение итоговой информации по разбитию заказа <see cref="PartitionedOrderByOrganizations"/>
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="organizationChoice">Данные для заказа, необходимые для обработки</param>
		/// <returns>Итоговая информация по разбитию заказа</returns>
		PartitionedOrderByOrganizations GetOrderPartsByOrganizations(
			IUnitOfWork uow, TimeSpan requestTime, OrderOrganizationChoice organizationChoice);
		/// <summary>
		/// Проверка наличия товаров, продающихся от разных организаций в одном заказе
		/// </summary>
		/// <param name="nomenclatureIds">Список Id номенклатур, находящихся в заказе</param>
		/// <returns><c>true</c> если заказ содержит товары от нескольких организаций <c>false</c> если от одной</returns>
		bool OrderHasGoodsFromSeveralOrganizations(IList<int> nomenclatureIds);
	}
}
