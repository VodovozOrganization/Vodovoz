using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

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

		/// <summary>
		/// Получить данные по районам, в которых используется правило доставки
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="rule">DeliveryPriceRule</param>
		/// <returns>Список массивов строк, где: 
		/// string[0] - название района; 
		/// string[1] - название DistrictSet; 
		/// string[2] - дата создания DistrictSet</returns>
		public List<string[]> GetDistrictNameDistrictSetNameAndCreationDateByDeliveryPriceRule(IUnitOfWork uow, DeliveryPriceRule rule)
		{
			CommonDistrictRuleItem districtRuleItemAlias = null;
			District districtAlias = null;
			DistrictsSet districtSetAlias = null;

			ProjectionList projectionList = Projections.ProjectionList()
				.Add(Projections.Property(() => districtAlias.DistrictName))
				.Add(Projections.Property(() => districtSetAlias.Name))
				.Add(Projections.Property(() => districtSetAlias.DateCreated));

			var districtsList = uow.Session.QueryOver(() => districtRuleItemAlias)
				.Where(d => d.DeliveryPriceRule.Id == rule.Id)
				.JoinAlias(d => d.District, () => districtAlias)
				.JoinAlias(() => districtAlias.DistrictsSet, () => districtSetAlias)
				.Select(projectionList)
				.List<object[]>()
				.Select(d => new string[] { d[0] as string, d[1].ToString(), ((DateTime)d[2]).ToShortDateString() });

			return districtsList.ToList();
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
