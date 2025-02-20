﻿using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Settings
{
	public class PaymentTypeOrganizationSettingsMap : ClassMap<PaymentTypeOrganizationSettings>
	{
		public PaymentTypeOrganizationSettingsMap()
		{
			Table("payment_types_organizations_settings");
			DiscriminateSubClassesOnColumn("payment_type");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			HasManyToMany(x => x.Organizations)
				.Table("payment_types_orgs_settings_organizations")
				.ParentKeyColumn("payment_types_organizations_settings_id")
				.ChildKeyColumn("organization_id");
		}
	}
}
