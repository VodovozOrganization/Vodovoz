using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class RoboAtsCounterpartyPatronymicMap : ClassMap<RoboAtsCounterpartyPatronymic>
	{
		public RoboAtsCounterpartyPatronymicMap()
		{
			Table("roboats_counterparty_patronymic_codes");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Patronymic).Column("patronymic");
			Map(x => x.Accent).Column("accent");
			Map(x => x.RoboatsAudiofile).Column("audio_filename");
			Map(x => x.FileId).Column("file_id");
		}
	}
}
