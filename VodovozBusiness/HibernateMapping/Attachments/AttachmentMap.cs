using FluentNHibernate.Mapping;
using Vodovoz.Domain.Attachments;

namespace Vodovoz.HibernateMapping.Attachments
{
	public class AttachmentMap : ClassMap<Attachment>
	{
		public AttachmentMap()
		{
			Table("files");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.FileName).Column("file_name");
			Map(x => x.EntityType).Column("entity_type").CustomType<EntityTypeStringType>();
			Map(x => x.EntityId).Column("entity_id");
			Map(x => x.ByteFile).Column("file").CustomSqlType("BinaryBlob").LazyLoad();
		}
	}
}
