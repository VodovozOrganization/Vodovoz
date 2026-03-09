using System.Collections.Generic;
using NHibernate;
using QS.DomainModel.UoW;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.EntityRepositories.Nodes;

namespace VodovozBusiness.EntityRepositories.Orders
{
	public interface IOnlineOrderTemplateRepository
	{
		int GetOnlineOrdersTemplatesCount(IUnitOfWork uow, int counterpartyId);
		OnlineOrderTemplateInfo GetOnlineOrderTemplateDataByTemplateId(IUnitOfWork uow, int templateId);
		IEnumerable<OnlineOrderTemplateCardForListData> GetOnlineOrdersTemplatesDataByCounterpartyId(
			IUnitOfWork uow, int counterpartyId, int skip, int take);
		IEnumerable<OnlineOrderTemplateProduct> GetOnlineOrdersTemplatesProductsByTemplateId(IUnitOfWork uow, int templateId);
		IEnumerable<OnlineOrderTemplateWeekdayData> GetOnlineOrdersTemplatesWeekdaysData(
			IUnitOfWork uow, int counterpartyId, int skip, int take);
		IEnumerable<OnlineOrderTemplateWeekdayData> GetOnlineOrdersTemplatesWeekdaysDataByTemplateId(
			IUnitOfWork uow, int templateId);
		IEnumerable<string> GetOnlineOrdersTemplatesWeekdaysByTemplateId(IUnitOfWork uow, int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateDataByTemplateId(int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateCounterpartyDataByTemplateId(int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateDeliveryPointDataByTemplateId(int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateWeekdaysByTemplateId(int templateId);
		IQueryOver GetQueryOverOnlineOrderTemplateProductsByTemplateId(int templateId);
	}
}
