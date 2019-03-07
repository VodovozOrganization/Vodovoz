using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.StoredResources;

namespace Vodovoz.HibernateMapping
{
	public class StoredResourceMap : ClassMap<StoredResource>
	{
		public StoredResourceMap()
		{
			Table("stored_resource");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.BinaryFile).Column("binary_file").CustomSqlType("BinaryBlob").LazyLoad();

			DiscriminateSubClassesOnColumn("type");
			Map(x => x.Type).Column("type").CustomType<ResoureceFileStringType>().Update().Not.Insert(); ;
		}
	}

	public class StoredImageResourceMap : SubclassMap<StoredImageResource>
	{
		public StoredImageResourceMap()
		{
			DiscriminatorValue(ResoureceType.Image.ToString());
		}
	}
}
