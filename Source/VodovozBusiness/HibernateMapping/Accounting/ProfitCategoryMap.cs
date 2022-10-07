using FluentNHibernate.Mapping;
using Vodovoz.Domain.Payments;
namespace Vodovoz.HibernateMapping.Accounting
{
	public class ProfitCategoryMap : ClassMap<ProfitCategory>
	{
		public ProfitCategoryMap()
		{
			Table("payment_profit_category");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}
