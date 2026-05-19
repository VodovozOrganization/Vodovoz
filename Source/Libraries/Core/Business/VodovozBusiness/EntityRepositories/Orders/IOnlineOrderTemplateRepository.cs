using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.EntityRepositories.Nodes;
using VodovozBusiness.Nodes;

namespace VodovozBusiness.EntityRepositories.Orders
{
	public interface IOnlineOrderTemplateRepository
	{
		Task<OnlineOrdersTemplatesData> GetOnlineOrdersTemplatesDataAsync(IUnitOfWork uow, int[] templatesIds);
		Task<AggregateOnlineOrderTemplateInfo> GetAggregateOnlineOrderTemplateInfoAsync(IUnitOfWork uow, int templateId);
		int GetOnlineOrdersTemplatesCount(IUnitOfWork uow, int counterpartyId);
		IEnumerable<OnlineOrderTemplate> GetActiveOnlineOrdersTemplatesForCreateOrders(IUnitOfWork uow, DateTime date);
		OnlineOrderTemplateInfo GetOnlineOrderTemplateDataByTemplateId(IUnitOfWork uow, int templateId);
		IEnumerable<OnlineOrderTemplateCardForListData> GetOnlineOrdersTemplatesDataByCounterpartyId(
			IUnitOfWork uow, int counterpartyId, int skip, int take);
		IEnumerable<OnlineOrderTemplateProduct> GetOnlineOrdersTemplatesProductsByTemplateId(IUnitOfWork uow, int templateId);
		IEnumerable<OnlineOrderTemplateWeekdayData> GetOnlineOrdersTemplatesWeekdaysData(
			IUnitOfWork uow, int counterpartyId, int skip, int take);
		IEnumerable<OnlineOrderTemplateWeekdayData> GetOnlineOrdersTemplatesWeekdaysDataByTemplateId(
			IUnitOfWork uow, int templateId);
		IEnumerable<string> GetOnlineOrdersTemplatesWeekdaysByTemplateId(IUnitOfWork uow, int templateId);
	}
}
