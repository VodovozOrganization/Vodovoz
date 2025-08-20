using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TaxcomEdoSettingsMap : ClassMap<TaxcomEdoSettings>
	{
		public TaxcomEdoSettingsMap()
		{
			Table("taxcom_edo_settings");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.Login).Column("login");
			Map(x => x.Password).Column("password");
			Map(x => x.EdoAccount).Column("edo_account");
			Map(x => x.OrganizationId).Column("organization_id");
		}
	}
}
