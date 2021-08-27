using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.EntityRepositories.Sale
{
	public class DistrictRuleRepository : IDistrictRuleRepository
	{
		public QueryOver<DeliveryPriceRule> GetQueryOverWithAllDeliveryPriceRules()
		{
			var res = QueryOver.Of<DeliveryPriceRule>();
			return res;
		}

		public IList<DeliveryPriceRule> GetAllDeliveryPriceRules(IUnitOfWork uow)
		{
			var res = GetQueryOverWithAllDeliveryPriceRules().GetExecutableQueryOver(uow.Session).List();
			return res;
		}
		
		public IList<CommonDistrictRuleItem> GetCommonDistrictRuleItemsForDistrict(IUnitOfWork uow, SectorDeliveryRuleVersion deliveryRuleVersion)
		{
			var res = uow.Session.QueryOver<CommonDistrictRuleItem>()
				.Where(i => i.SectorDeliveryRuleVersion.Id == deliveryRuleVersion.Id)
				.List();
			return res;
		}
		
		public IList<SectorVersion> GetDistrictsHavingRule(IUnitOfWork uow, DeliveryPriceRule rule)
		{
			SectorVersion sectorVersionAlias = null;
			CommonDistrictRuleItem commonDistrictRuleItemAlias = null;
			SectorDeliveryRuleVersion sectorDeliveryRuleVersionAlias = null;
			var res = uow.Session.QueryOver(() => commonDistrictRuleItemAlias)
				.JoinAlias(() => sectorDeliveryRuleVersionAlias, () => commonDistrictRuleItemAlias.SectorDeliveryRuleVersion)
				.JoinEntityAlias(() => sectorVersionAlias,
					() => sectorVersionAlias.Sector.Id == sectorDeliveryRuleVersionAlias.Sector.Id &&
					      sectorVersionAlias.Status == SectorsSetStatus.Active,
					JoinType.LeftOuterJoin)
				.Where(d => d.DeliveryPriceRule.Id == rule.Id)
						 .List()
						 .Select(x => sectorVersionAlias)
						 .ToList();

			return res;
		}
	}
}
