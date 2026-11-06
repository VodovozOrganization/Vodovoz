using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class AssignedChildMap : ClassMap<AssignedChild>
	{
		public AssignedChildMap()
		{
			Table("bulk_assigned_children");
			Id(x => x.Id).Column("id").GeneratedBy.Assigned();
			Map(x => x.Name).Column("name");
		}
	}
}
