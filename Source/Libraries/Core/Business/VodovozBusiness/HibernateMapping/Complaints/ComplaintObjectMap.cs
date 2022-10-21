using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.HibernateMapping.Complaints
{
	public class ComplaintObjectMap : ClassMap<ComplaintObject>
	{
		public ComplaintObjectMap()
		{
			Table("complaint_objects");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreateDate).Column("create_date").ReadOnly();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}
