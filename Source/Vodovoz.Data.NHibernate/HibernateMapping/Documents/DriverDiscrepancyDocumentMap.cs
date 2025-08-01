using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class DriverDiscrepancyDocumentMap : ClassMap<DriverDiscrepancyDocument>
	{
		public DriverDiscrepancyDocumentMap()
		{
			Table("driver_discrepancy_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.TimeStamp).Column("time_stamp");
			Map(x => x.LastEditedTime).Column("last_edited_time");
			Map(x => x.AuthorId).Column("author_id");
			Map(x => x.LastEditorId).Column("last_editor_id");

			References(x => x.RouteList).Column("route_list_id");

			HasMany(x => x.Items).Cascade.AllDeleteOrphan().Inverse().KeyColumn("driver_discrepancy_document_id");
		}
	}
}
