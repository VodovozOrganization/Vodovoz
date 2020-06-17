using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;

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

		public static IList<CommonDistrictRuleItem> GetScheduleCommonDistrictRuleItemsForDistrict(IUnitOfWork uow, District district)
		{
			var res = uow.Session.QueryOver<CommonDistrictRuleItem>()
						 .Where(i => i.District.Id == district.Id)
						 .List();
			return res;
		}

		public static IList<District> GetDistrictsHavingRule(IUnitOfWork uow, DeliveryPriceRule rule)
		{
			var res = uow.Session.QueryOver<CommonDistrictRuleItem>()
						 .Where(d => d.DeliveryPriceRule.Id == rule.Id)
						 .List()
						 .Select(r => r.District)
						 .ToList();

			return res;
		}
	}
}
