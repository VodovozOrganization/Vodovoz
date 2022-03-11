using FluentNHibernate.Mapping;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.HibernateMapping.Organization
{
	public class RoboatsStreetMap : ClassMap<RoboatsStreet>
	{
		public RoboatsStreetMap()
		{
			Table("roboats_streets");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.Type).Column("type");
			Map(x => x.RoboatsAudiofile).Column("audio_filename");
			Map(x => x.FileId).Column("file_id");
		}
	}
}
