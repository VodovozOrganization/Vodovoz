using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class NomenclatureFileInformationMap : ClassMap<NomenclatureFileInformation>
	{
		public NomenclatureFileInformationMap()
		{
			Table("nomenclature_file_informations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.NomenclatureId).Column("nomenclature_id");
			Map(x => x.FileName).Column("file_name");
		}
	}
}
