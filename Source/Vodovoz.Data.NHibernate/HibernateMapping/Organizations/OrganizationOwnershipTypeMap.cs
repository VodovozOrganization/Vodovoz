using FluentNHibernate.Mapping;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Organizations
{
	public class OrganizationOwnershipTypeMap : ClassMap<OrganizationOwnershipType>
	{
		public OrganizationOwnershipTypeMap()
		{
			Table("organization_ownership_type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Abbreviation).Column("abbreviation");
			Map(x => x.FullName).Column("full_name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.Code).Column("code");
		}
	}
}
