using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
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
		public List<DeliveryPriceRuleDistrictRelation> GetDistrictNameDistrictSetNameAndCreationDateByDeliveryPriceRule(IUnitOfWork uow, DeliveryPriceRule rule)
		{
			CommonDistrictRuleItem districtRuleItemAlias = null;
			District districtAlias = null;
			DistrictsSet districtSetAlias = null;
			DeliveryPriceRuleDistrictRelation ruleDistrictRelationAlias = null;

			var districtsList = uow.Session.QueryOver(() => districtRuleItemAlias)
					.Where(d => d.DeliveryPriceRule.Id == 1)
					.JoinAlias(d => d.District, () => districtAlias)
					.JoinAlias(() => districtAlias.DistrictsSet, () => districtSetAlias)
					.SelectList(list => list
						.Select(() => districtAlias.DistrictName).WithAlias(() => ruleDistrictRelationAlias.DistrictName)
						.Select(() => districtSetAlias.Name).WithAlias(() => ruleDistrictRelationAlias.DistrictSetName)
						.Select(() => districtSetAlias.DateCreated).WithAlias(() => ruleDistrictRelationAlias.DistrictSetCreationDate)
					)
					.TransformUsing(Transformers.AliasToBean<DeliveryPriceRuleDistrictRelation>())
					.List<DeliveryPriceRuleDistrictRelation>();

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

	public class DeliveryPriceRuleDistrictRelation
	{
		public string DistrictName { get; set; }
		public string DistrictSetName { get; set; }
		public DateTime DistrictSetCreationDate { get; set; }
	}
}
