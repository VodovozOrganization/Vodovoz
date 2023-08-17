using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.HibernateMapping.Counterparty
{
	public class RoboAtsCounterpartyNameMap : ClassMap<RoboAtsCounterpartyName>
	{
		public RoboAtsCounterpartyNameMap()
		{
			Table("roboats_counterparty_name_codes");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.Accent).Column("accent");
			Map(x => x.RoboatsAudiofile).Column("audio_filename");
			Map(x => x.FileId).Column("file_id");
		}
	}
}
