using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Complaints
{
	public class ComplaintKindMap : ClassMap<ComplaintKind>
	{
		public ComplaintKindMap()
		{
			Table("complaint_kinds");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");

			References(x => x.ComplaintObject).Column("complaint_object_id");

			HasManyToMany(x => x.Subdivisions)
				.Table("complaint_kind_subdivisions")
				.ParentKeyColumn("complaint_kind_id")
				.ChildKeyColumn("subdivision_id")
				.LazyLoad();
		}
	}
}
