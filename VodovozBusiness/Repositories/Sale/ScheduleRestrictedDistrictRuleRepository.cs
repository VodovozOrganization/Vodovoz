using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Repositories.Sale
{
	public static class ScheduleRestrictedDistrictRuleRepository
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

		public static IList<ScheduleRestrictedDistrictRuleItem> GetScheduleRestrictedDistrictRuleItemsForDistrict(IUnitOfWork uow, ScheduleRestrictedDistrict district)
		{
			var res = uow.Session.QueryOver<ScheduleRestrictedDistrictRuleItem>()
						 .Where(i => i.ScheduleRestrictedDistrict.Id == district.Id)
						 .List();
			return res;
		}

		public static IList<ScheduleRestrictedDistrict> GetDistrictsHavingRule(IUnitOfWork uow, DeliveryPriceRule rule)
		{
			var res = uow.Session.QueryOver<ScheduleRestrictedDistrictRuleItem>()
						 .Where(d => d.DeliveryPriceRule.Id == rule.Id)
						 .List()
						 .Select(r => r.ScheduleRestrictedDistrict)
						 .ToList();

			return res;
		}
	}
}
