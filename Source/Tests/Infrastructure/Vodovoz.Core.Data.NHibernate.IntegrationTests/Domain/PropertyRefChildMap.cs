using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class PropertyRefChildMap : ClassMap<PropertyRefChild>
	{
		public PropertyRefChildMap()
		{
			Table("bulk_property_ref_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}
