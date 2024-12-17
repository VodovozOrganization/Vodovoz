using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Organizations
{
	public class OrganizationEntityMap : ClassMap<OrganizationEntity>
	{
		public OrganizationEntityMap()
		{
			Table("organizations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.FullName).Column("full_name");
			Map(x => x.INN).Column("INN");
			Map(x => x.KPP).Column("KPP");
			Map(x => x.OGRN).Column("OGRN");
			Map(x => x.OKPO).Column("OKPO");
			Map(x => x.OKVED).Column("OKVED");
			Map(x => x.Email).Column("email");
		}
	}
}
