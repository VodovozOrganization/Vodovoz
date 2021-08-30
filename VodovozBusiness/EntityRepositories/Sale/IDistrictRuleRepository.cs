using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.EntityRepositories.Sale
{
	public interface IDistrictRuleRepository
	{
		QueryOver<DeliveryPriceRule> GetQueryOverWithAllDeliveryPriceRules();
		IList<DeliveryPriceRule> GetAllDeliveryPriceRules(IUnitOfWork uow);
		IList<CommonSectorsRuleItem> GetCommonDistrictRuleItemsForDistrict(IUnitOfWork uow, SectorDeliveryRuleVersion deliveryRuleVersion);
		IList<SectorVersion> GetSectorsHavingRule(IUnitOfWork uow, DeliveryPriceRule rule);
	}
}