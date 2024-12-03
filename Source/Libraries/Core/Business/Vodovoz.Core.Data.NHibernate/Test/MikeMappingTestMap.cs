using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Test;

namespace Vodovoz.Core.Data.NHibernate.Test
{
	public class MikeMappingTestMap : SubclassMap<MikeMappingTest>
	{
		public MikeMappingTestMap()
		{
			Table("table_mapping_test");
			Extends(typeof(MappingTest));
		}
	}
}
