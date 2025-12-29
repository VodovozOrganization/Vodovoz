using FluentNHibernate.Mapping;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Organizations
{
	public class OrganizationMap : ClassMap<Organization>
	{
		public OrganizationMap()
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
			Map(x => x.EmailForMailing).Column("email_for_mailing");
			Map(x => x.DisableDebtMailing).Column("disable_debt_mailing");
			Map(x => x.DebtMailingWithSignature).Column("debt_mailing_with_signature");
			Map(x => x.IsNeedCashlessMovementControl).Column("is_need_cashless_movement_control");
			Map(x => x.CashBoxId).Column("cash_box_id");
			Map(x => x.AvangardShopId).Column("avangard_shop_id");
			Map(x => x.CashBoxTokenFromTrueMark).Column("edo_key");
			Map(x => x.OrganizationEdoType).Column("edo_type");
			Map(x => x.Suffix).Column("suffix");
			Map(x => x.Prefix).Column("prefix");
			Map(x => x.IsOsnoMode).Column("is_osno_mode");
			Map(x => x.IsUsnMode).Column("is_usn_mode");

			References(x => x.Stamp).Column("stamp_id");
			References(x => x.DefaultAccount).Column("default_account_id");
			
			HasOne(x => x.TaxcomEdoSettings)
				.PropertyRef(x => x.OrganizationId);

			HasMany(x => x.Accounts).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("org_id");
			HasMany(x => x.Phones).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("org_id");
			HasMany(x => x.OrganizationVersions).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("organization_id");
			HasMany(x => x.VatRateVersions).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("organization_id").OrderBy("start_date DESC");
		}
	}
}
