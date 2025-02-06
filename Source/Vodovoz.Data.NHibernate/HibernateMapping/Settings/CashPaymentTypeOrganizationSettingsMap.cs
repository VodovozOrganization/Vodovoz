using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Settings
{
	public class CashPaymentTypeOrganizationSettingsMap : SubclassMap<CashPaymentTypeOrganizationSettings>
	{
		public CashPaymentTypeOrganizationSettingsMap()
		{
			DiscriminatorValue(nameof(PaymentType.Cash));
		}
	}
}
