using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Test;

namespace Vodovoz.Core.Data.NHibernate.Test
{
	public class MappingTestMap : ClassMap<MappingTest>
	{
		public MappingTestMap()
		{
			Table("table_mapping_test");

			Id(x => x.Name).Column("name").GeneratedBy.Assigned();

			Map(x => x.Message).Column("message");
			Map(x => x.Description).Column("description");

			//UseUnionSubclassForInheritanceMapping();
		}
	}
}
