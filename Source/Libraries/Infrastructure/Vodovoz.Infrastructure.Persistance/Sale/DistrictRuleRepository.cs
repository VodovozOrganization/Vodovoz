using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Sale;

namespace Vodovoz.Infrastructure.Persistance.Sale
{
	internal sealed class DistrictRuleRepository : IDistrictRuleRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public DistrictRuleRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		/// <inheritdoc/>
		public bool SameDeliveryPriceRuleExists(IUnitOfWork uow, DeliveryPriceRule rule)
		{
			var query =
				from deliveryPriceRule in uow.Session.Query<DeliveryPriceRule>()
				where deliveryPriceRule.Water19LCount == rule.Water19LCount
					&& deliveryPriceRule.Water6LCount == rule.Water6LCount
					&& deliveryPriceRule.Water1500mlCount == rule.Water1500mlCount
					&& deliveryPriceRule.Water600mlCount == rule.Water600mlCount
					&& deliveryPriceRule.Water500mlCount == rule.Water500mlCount
					&& deliveryPriceRule.OrderMinSumEShopGoods == rule.OrderMinSumEShopGoods
					&& deliveryPriceRule.Id != rule.Id
				select deliveryPriceRule.Id;

			return query.Any();
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
		/// <returns>Название района, название версии районов, в которую входит район, дата создания версии районов</returns>
		public List<DistrictAndDistrictSet> GetDistrictNameDistrictSetNameAndCreationDateByDeliveryPriceRule(IUnitOfWork uow, DeliveryPriceRule rule)
		{
			DistrictRuleItemBase districtRuleItemAlias = null;
			District districtAlias = null;
			DistrictsSet districtSetAlias = null;
			DistrictAndDistrictSet ruleDistrictRelationAlias = null;

			var districtsList = uow.Session.QueryOver(() => districtRuleItemAlias)
					.Where(d => d.DeliveryPriceRule.Id == rule.Id)
					.JoinAlias(d => d.District, () => districtAlias)
					.JoinAlias(() => districtAlias.DistrictsSet, () => districtSetAlias)
					.SelectList(list => list
						.Select(() => districtAlias.DistrictName).WithAlias(() => ruleDistrictRelationAlias.DistrictName)
						.Select(() => districtSetAlias.Name).WithAlias(() => ruleDistrictRelationAlias.DistrictSetName)
						.Select(() => districtSetAlias.DateCreated).WithAlias(() => ruleDistrictRelationAlias.DistrictSetCreationDate)
					)
					.TransformUsing(Transformers.AliasToBean<DistrictAndDistrictSet>())
					.List<DistrictAndDistrictSet>();

			return districtsList.Distinct(new DistrictAndDistrictSetComparer()).ToList();
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
