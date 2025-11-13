using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Payments
{
	public class NotAllocatedCounterpartyMap : ClassMap<NotAllocatedCounterparty>
	{
		public NotAllocatedCounterpartyMap()
		{
			Table("not_allocated_counterparties");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Inn).Column("inn");
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");

			References(x => x.ProfitCategory).Column("profit_category_id");
		}
	}
}
