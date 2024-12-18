using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Organizations
{
	public class OrganizationVersionMap : ClassMap<OrganizationVersionEntity>
	{
		public OrganizationVersionMap()
		{
			Table("organization_versions");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Address)
				.Column("address");

			Map(x => x.JurAddress)
				.Column("jur_address");

			Map(x => x.StartDate)
				.Column("start_date");

			Map(x => x.EndDate)
				.Column("end_date");

			References(x => x.Leader)
				.Column("leader_id");

			References(x => x.Accountant)
				.Column("accountant_id");

			References(x => x.SignatureLeader)
				.Column("signature_leader_id");

			References(x => x.SignatureAccountant)
				.Column("signature_accountant_id");

			References(x => x.Organization)
				.Column("organization_id");
		}
	}
}
