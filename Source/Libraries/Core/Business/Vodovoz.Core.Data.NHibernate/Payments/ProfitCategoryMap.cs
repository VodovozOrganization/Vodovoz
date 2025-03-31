using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Core.Data.NHibernate.Payments
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
