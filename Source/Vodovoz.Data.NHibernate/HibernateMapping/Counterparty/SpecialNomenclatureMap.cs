using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
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
