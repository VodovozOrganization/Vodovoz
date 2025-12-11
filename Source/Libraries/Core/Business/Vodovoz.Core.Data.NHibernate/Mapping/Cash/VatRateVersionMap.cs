using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Cash
{
	public class VatRateVersionMap : ClassMap<VatRateVersion>
	{
		public VatRateVersionMap()
		{
			Table("vat_rate_versions");
			
			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			
			References(x => x.Organization).Column("organization_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.VatRate).Column("vat_rate_id");
		}
	}
}
