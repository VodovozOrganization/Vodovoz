using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Cash
{
	public class VatRateMap : ClassMap<VatRate>
	{
		public VatRateMap()
		{
			Table("vat_rates");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.VatRateValue).Column("vat_rate_value");
			Map(x => x.Vat1cTypeValue).Column("vat_rate_1c_type");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}
