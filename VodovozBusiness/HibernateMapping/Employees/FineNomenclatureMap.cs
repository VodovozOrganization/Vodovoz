using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HMap
{
	public class FineNomenclatureMap : ClassMap<FineNomenclature>
	{
		public FineNomenclatureMap ()
		{
			Table ("fines_nomenclatures");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("amount");
			References(x => x.Fine).Column("fine_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}