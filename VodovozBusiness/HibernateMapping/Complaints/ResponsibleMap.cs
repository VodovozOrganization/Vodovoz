using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Complaints
{
	public class ResponsibleMap : ClassMap<Responsible>
	{
		public ResponsibleMap()
		{
			Table("responsible");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.IsArchived).Column("is_archived");
		}
	}
}
