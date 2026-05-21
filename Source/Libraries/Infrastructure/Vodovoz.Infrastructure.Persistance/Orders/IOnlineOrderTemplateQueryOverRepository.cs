using NHibernate;

namespace Vodovoz.Infrastructure.Persistance.Orders
{
	public interface IOnlineOrderTemplateQueryOverRepository
	{
		IQueryOver GetQueryOverOnlineOrderTemplateDataByTemplateId(int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateData(int[] templatesIds);
		IQueryOver GetQueryOverOnlineOrderTemplateCounterpartyDataByTemplateId(int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateDeliveryPointDataByTemplateId(int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateWeekdaysByTemplateId(int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateWeekdays(int[] templatesIds);
		IQueryOver GetQueryOverOnlineOrderTemplateProductsByTemplateId(int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateProducts(int[] templatesIds);
	}
}
