using System.Collections.Generic;
using NHibernate;

namespace VodovozBusiness.EntityRepositories.Orders
{
	public interface IOnlineOrderTemplateQueryOverRepository
	{
		//IQueryOver GetQueryOverOnlineOrderTemplateDataByTemplateId(int templateId);
		/// <summary>
		/// Получение запроса для выгрузки шаблонов
		/// </summary>
		/// <param name="templatesIds">Идентификаторы шаблонов</param>
		/// <returns></returns>
		IQueryOver GetQueryOverOnlineOrderTemplates(IEnumerable<int> templatesIds);
		IQueryOver GetQueryOverOnlineOrderTemplateCounterpartyDataByTemplateId(int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateDeliveryPointDataByTemplateId(int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateWeekdaysByTemplateId(int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateWeekdays(IEnumerable<int> templatesIds);
		IQueryOver GetQueryOverOnlineOrderTemplateSaleItemsByTemplateId(int templateId);

		/// <summary>
		/// Получение запроса для выгрузки продаваемых позиций шаблона <see cref="OnlineOrderTemplateSaleItem"/>
		/// </summary>
		/// <param name="templatesIds">Идентификаторы шаблонов</param>
		/// <returns></returns>
		IQueryOver GetQueryOverOnlineOrderTemplateSaleItems(IEnumerable<int> templatesIds);
		
		/// <summary>
		/// Получение запроса для выгрузки промонаборов шаблона <see cref="OnlineOrderTemplatePromoSet"/>
		/// </summary>
		/// <param name="templatesIds">Идентификаторы шаблонов</param>
		/// <returns></returns>
		IQueryOver GetQueryOverOnlineOrderTemplatePromoSets(IEnumerable<int> templatesIds);
		
		/// <summary>
		/// Получение запроса для выгрузки бесплатных пакетов аренды шаблона <see cref="OnlineOrderTemplateFreeRentPackage"/>
		/// </summary>
		/// <param name="templatesIds">Идентификаторы шаблонов</param>
		/// <returns></returns>
		IQueryOver GetQueryOverOnlineOrderTemplateFreeRentPackages(IEnumerable<int> templatesIds);

		/// <summary>
		/// Получение запроса для выгрузки фиксы точек доставки шаблонов
		/// </summary>
		/// <param name="templatesIds">Идентификаторы шаблонов</param>
		/// <returns></returns>
		IQueryOver GetQueryOverOnlineOrderTemplateDeliveryPointFixedPrices(IEnumerable<int> templatesIds);

		/// <summary>
		/// Получение запроса для выгрузки фиксы точек клиентов
		/// </summary>
		/// <param name="templatesIds">Идентификаторы шаблонов</param>
		/// <returns></returns>
		IQueryOver GetQueryOverOnlineOrderTemplateCounterpartyFixedPrices(IEnumerable<int> templatesIds);
	}
}
