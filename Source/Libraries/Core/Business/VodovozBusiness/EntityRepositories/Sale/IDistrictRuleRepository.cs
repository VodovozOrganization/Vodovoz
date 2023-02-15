using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.EntityRepositories.Sale
{
	public interface IDistrictRuleRepository
	{
		QueryOver<DeliveryPriceRule> GetQueryOverWithAllDeliveryPriceRules();
		IList<DeliveryPriceRule> GetAllDeliveryPriceRules(IUnitOfWork uow);
		IList<CommonDistrictRuleItem> GetCommonDistrictRuleItemsForDistrict(IUnitOfWork uow, District district);
		List<DistrictAndDistrictSet> GetDistrictNameDistrictSetNameAndCreationDateByDeliveryPriceRule(IUnitOfWork uow, DeliveryPriceRule rule);
		IList<District> GetDistrictsHavingRule(IUnitOfWork uow, DeliveryPriceRule rule);
	}
}
