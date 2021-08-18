using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Repositories.Sale
{
	public static class DistrictRuleRepository
	{
		public static QueryOver<DeliveryPriceRule> GetQueryOverWithAllDeliveryPriceRules()
		{
			var res = QueryOver.Of<DeliveryPriceRule>();
			return res;
		}

		public static IList<DeliveryPriceRule> GetAllDeliveryPriceRules(IUnitOfWork uow)
		{
			var res = GetQueryOverWithAllDeliveryPriceRules().GetExecutableQueryOver(uow.Session).List();
			return res;
		}

		public static IList<CommonDistrictRuleItem> GetCommonDistrictRuleItemsForDistrict(IUnitOfWork uow, Sector _sector)
		{
			var res = uow.Session.QueryOver<CommonDistrictRuleItem>()
						 .Where(i => i.SectorDeliveryRuleVersion.Id == _sector.Id)
						 .List();
			return res;
		}

		public static IList<SectorVersion> GetDistrictsHavingRule(IUnitOfWork uow, DeliveryPriceRule rule)
		{
			SectorVersion sectorVersionAlias = null;
			CommonDistrictRuleItem commonDistrictRuleItemAlias = null;
			SectorDeliveryRuleVersion sectorDeliveryRuleVersionAlias = null;
			var res = uow.Session.QueryOver(() => commonDistrictRuleItemAlias)
				.JoinAlias(() => sectorDeliveryRuleVersionAlias, () => commonDistrictRuleItemAlias.SectorDeliveryRuleVersion)
				.JoinEntityAlias(() => sectorVersionAlias,
					() => sectorVersionAlias.Sector == sectorDeliveryRuleVersionAlias.Sector &&
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
