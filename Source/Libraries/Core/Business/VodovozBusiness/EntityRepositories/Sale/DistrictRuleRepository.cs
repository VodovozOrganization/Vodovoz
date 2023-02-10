using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.HibernateMapping.Logistic;

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

		public IList<CommonDistrictRuleItem> GetCommonDistrictRuleItemsForDistrict(IUnitOfWork uow, District district)
		{
			var res = uow.Session.QueryOver<CommonDistrictRuleItem>()
						 .Where(i => i.District.Id == district.Id)
						 .List();
			return res;
		}

		public IDictionary<District, DistrictsSet> GetDistrictsHavingRuleWithDistrictSetVersion(IUnitOfWork uow, DeliveryPriceRule rule)
		{
			CommonDistrictRuleItem commonDistrictRuleItemAlias = null;
			District districtAlias = null;
			DeliveryPriceRule deliveryPriceRuleAlias = null;
			DistrictsSet districtsSetAlias = null;

			var query = uow.Session.QueryOver(() => commonDistrictRuleItemAlias)
				.JoinAlias(c => c.DeliveryPriceRule, () => deliveryPriceRuleAlias)
				.Where(() => deliveryPriceRuleAlias.Id == rule.Id)
				.JoinAlias(c => c.District, () => districtAlias)
				.JoinAlias(() => districtAlias.DistrictsSet, () => districtsSetAlias)
				.List() ;


			//Получаем все CommonDistrictRuleItem у которых DeliveryPriceRule.Id == rule.Id
			var districtRuleItems = uow.Session.QueryOver<CommonDistrictRuleItem>()
				.Where(i => i.DeliveryPriceRule.Id == rule.Id)
				.List();

			//Получаем все найденные District районы
			var desiredDistrics = districtRuleItems.Select(d => d.District).ToList();

			//Из найденных районов получаем версии районов
			var versions = desiredDistrics.Select(d => d.DistrictsSet).ToList();

			ICriteria districts = uow.Session.CreateCriteria<District>();
			DetachedCriteria desiredDistrictIds = DetachedCriteria.For<CommonDistrictRuleItem>()
				.SetProjection(Projections.Property(nameof(CommonDistrictRuleItem.District)))
				.Add(Restrictions.Eq(nameof(CommonDistrictRuleItem.DeliveryPriceRule), rule.Id));

			districts.Add(Subqueries.PropertyIn(nameof(District.Id), desiredDistrictIds));

			
			var districtItems = districts.List();

			//	ICriteria consult = Session.CreateCriteria<Car>();
			//DetachedCriteria c = DetachedCriteria.For<Employee>()
			//   .SetProjection(Projections.Property("Companies"))
			//   .Add(Restrictions.Eq("Id", employee.Id));
			//consult.Add(Subqueries.PropertyIn("Company.Id", c));

			//var res = uow.Session.QueryOver<CommonDistrictRuleItem>()
			//			 .Where(d => d.DeliveryPriceRule.Id == rule.Id)
			//			 .List()
			//			 .Select(r => r.District)
			//			 .ToList()
			//			 .Select(d=>d.DistrictsSet)
			//			 .ToList();

			//return res;
			return new Dictionary<District, DistrictsSet>();
		}

		public IList<District> GetDistrictsHavingRule(IUnitOfWork uow, DeliveryPriceRule rule)
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
