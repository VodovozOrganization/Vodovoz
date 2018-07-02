using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping.Goods
{
	public class Folder1cMap : ClassMap<Folder1c>
	{
		public Folder1cMap()
		{
			Table("nomenclature_1c_folders");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.Code1c).Column("code1c");
		}
	}
}

