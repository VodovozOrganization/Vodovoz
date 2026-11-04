using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class NonInverseChildMap : ClassMap<NonInverseChild>
	{
		public NonInverseChildMap()
		{
			Table("bulk_non_inverse_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}
