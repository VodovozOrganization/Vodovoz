using FluentNHibernate.Mapping;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Fuel
{
	public class FuelLimitMap : ClassMap<FuelLimit>
	{
		public FuelLimitMap()
		{
			Table("fuel_limits");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.LimitId).Column("limit_id");
			Map(x => x.CardId).Column("card_id");
			Map(x => x.ContractId).Column("contract_id");
			Map(x => x.ProductGroup).Column("product_group");
			Map(x => x.ProductType).Column("product_type");
			Map(x => x.Amount).Column("amount_liters");
			Map(x => x.UsedAmount).Column("used_amount_liters");
			Map(x => x.Sum).Column("sum");
			Map(x => x.UsedSum).Column("used_sum");
			Map(x => x.Unit).Column("unit");
			Map(x => x.TransctionsCount).Column("transctions_count");
			Map(x => x.TransactionsOccured).Column("transctions_occured");
			Map(x => x.Period).Column("period");
			Map(x => x.PeriodUnit).Column("period_unit");
			Map(x => x.TermType).Column("term_type");
			Map(x => x.CreateDate).Column("create_date");
			Map(x => x.LastEditDate).Column("last_edit_date");
			Map(x => x.Status).Column("status");
		}
	}
}
