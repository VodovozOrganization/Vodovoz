using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Documents
{
	public class CarLoadDocumentMap : ClassMap<CarLoadDocumentEntity>
	{
		public CarLoadDocumentMap()
		{
			Table("store_car_load_documents");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			OptimisticLock.Version();
			Version(x => x.Version)
				.Column("version");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.LoadOperationState)
				.Column("load_operation_state");

			References(x => x.Author)
				.Column("author_id");

			References(x => x.RouteList)
				.Column("route_list_id");

			HasMany(x => x.Items)
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.KeyColumn("car_load_document_id");
		}
	}
}
