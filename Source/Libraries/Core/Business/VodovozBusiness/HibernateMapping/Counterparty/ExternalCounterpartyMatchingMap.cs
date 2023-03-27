using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
	public class ExternalCounterpartyMatchingMap : ClassMap<ExternalCounterpartyMatching>
	{
		public ExternalCounterpartyMatchingMap()
		{
			Table("external_counterparties_matching");
			
			OptimisticLock.Version();
			Version(x => x.Version).Column("version");
			
			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Created).Column("created");
			Map(x => x.ExternalCounterpartyId).Column("external_counterparty_guid");
			Map(x => x.PhoneNumber).Column("phone_number");
			Map(x => x.Status).Column("status");
			Map(x => x.CounterpartyFrom).Column("counterparty_from");

			References(x => x.AssignedExternalCounterparty)
				.Column("assigned_external_counterparty_id")
				.Cascade.AllDeleteOrphan();
			References(x => x.ExistingExternalCounterpartyWithSameParams)
				.Column("existing_external_counterparty_with_same_params_id");
		}
	}
}
