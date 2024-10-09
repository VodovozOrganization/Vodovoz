using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Data.NHibernate.Goods
{
	public class NomenclatureMap : ClassMap<NomenclatureEntity>
	{
		public NomenclatureMap()
		{
			Table("nomenclature");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.IsAccountableInTrueMark).Column("is_accountable_in_chestniy_znak");
			Map(x => x.Gtin).Column("gtin");
		}
	}
}
