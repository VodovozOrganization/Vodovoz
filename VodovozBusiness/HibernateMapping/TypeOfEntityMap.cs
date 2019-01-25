using FluentNHibernate.Mapping;

namespace Vodovoz.HibernateMapping
{
	public class TypeOfEntityMap : ClassMap<TypeOfEntity>
	{
		public TypeOfEntityMap()
		{
			Table("entity_types");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CustomName).Column("entity_custom_name");
			Map(x => x.Type).Column("entity_type");
		}
	}
}
