using FluentNHibernate.Mapping;
using Vodovoz.Domain.Payments;
namespace Vodovoz.HibernateMapping.Accounting
{
	public class CategoryProfitMap : ClassMap<CategoryProfit>
	{
		public CategoryProfitMap()
		{
			Table("payment_profit_category");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}
