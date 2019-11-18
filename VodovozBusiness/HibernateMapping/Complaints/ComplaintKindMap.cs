using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.HibernateMapping.Complaints
{
	public class ComplaintKindMap : ClassMap<ComplaintKind>
	{
		public ComplaintKindMap()
		{
			Table("complaint_kinds");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}
