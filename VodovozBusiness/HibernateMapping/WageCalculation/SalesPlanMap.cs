using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.HibernateMapping.WageCalculation
{
	public class SalesPlanMap : ClassMap<SalesPlan>
	{
		public SalesPlanMap()
		{
			Table("sales_plans");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.EmptyBottlesToTake).Column("empty_bottles_to_take_salesplan_wage");
			Map(x => x.FullBottleToSell).Column("full_bottles_to_sell_salesplan_wage");
		}
	}
}