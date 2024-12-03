using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Test;

namespace Vodovoz.Core.Data.NHibernate.Test
{
	public class PeterMappingTestMap : SubclassMap<PeterMappingTest>
	{
		public PeterMappingTestMap()
		{
			Table("table_mapping_test");
			Extends(typeof(MappingTest));
		}
	}
}
