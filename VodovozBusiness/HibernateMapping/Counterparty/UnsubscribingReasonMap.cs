using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
	public class UnsubscribingReasonMap : ClassMap<UnsubscribingReason>
	{
		public UnsubscribingReasonMap()
		{
			Table("unsubscribing_reasons");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.IsOtherReason).Column("is_other_reason");

		}
	}
}
