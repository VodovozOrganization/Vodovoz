using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
	public class SpecialNomenclatureMap : ClassMap<SpecialNomenclature>
	{
		public SpecialNomenclatureMap()
		{
			Table("special_nomenclature");
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.SpecialId).Column("special_id");

			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
