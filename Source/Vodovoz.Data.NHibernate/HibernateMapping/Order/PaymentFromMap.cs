using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class PaymentFromMap : ClassMap<PaymentFrom>
	{
		public PaymentFromMap()
		{
			Table("payments_from");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.ReceiptRequired).Column("receipt_required");
			Map(x => x.OrganizationSettingsCriterion).Column("organization_criterion");
			Map(x => x.OnlineCashBoxRegistrationRequired).Column("online_cashbox_registration_required");
			Map(x => x.RegistrationInAvangardRequired).Column("registration_in_avangard_required");
			Map(x => x.RegistrationInTaxcomRequired).Column("registration_in_taxcom_required");
		}
	}
}
