using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class ManufacturerMap : ClassMap<Manufacturer>
	{
		public ManufacturerMap()
		{
			Table("manufacturers");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}

