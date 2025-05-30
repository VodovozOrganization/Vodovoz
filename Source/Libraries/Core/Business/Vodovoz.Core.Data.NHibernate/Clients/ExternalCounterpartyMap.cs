using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Data.NHibernate.Clients
{
	public class ExternalCounterpartyMap : ClassMap<ExternalCounterpartyEntity>
	{
		public ExternalCounterpartyMap()
		{
			Table(ExternalCounterpartyEntity.TableName);
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
