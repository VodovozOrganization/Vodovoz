using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class ExternalCounterpartyMap : ClassMap<ExternalCounterparty>
	{
		public ExternalCounterpartyMap()
		{
			Table("external_counterparties");
			DiscriminateSubClassesOnColumn("counterparty_from");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ExternalCounterpartyId).Column("external_counterparty_id");
			Map(x => x.CounterpartyFrom).Column("counterparty_from").ReadOnly();
			Map(x => x.CreationDate).Column("creation_date").ReadOnly();
			Map(x => x.IsArchive).Column("is_archive");

			References(x => x.Phone).Column("phone_id").Cascade.AllDeleteOrphan();
			References(x => x.Email).Column("email_id").Cascade.AllDeleteOrphan();
		}
	}
}
