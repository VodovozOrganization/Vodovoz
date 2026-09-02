using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class OutgoingEdoDocumentMap : ClassMap<OutgoingEdoDocument>
	{
		public OutgoingEdoDocumentMap()
		{
			Table("edo_outgoing_documents");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.CreationTime)
				.Column("creation_time")
				.ReadOnly();

			Map(x => x.Type)
				.Column("type")
				.ReadOnly();

			Map(x => x.EdoType)
				.Column("edo_type");

			Map(x => x.DocumentType)
				.Column("document_type");

			Map(x => x.Status)
				.Column("status");

			Map(x => x.SendTime)
				.Column("send_time");

			Map(x => x.AcceptTime)
				.Column("accept_time");
		}
	}
}
