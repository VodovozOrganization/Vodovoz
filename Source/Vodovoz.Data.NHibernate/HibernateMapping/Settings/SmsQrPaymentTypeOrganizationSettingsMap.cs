using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Settings
{
	public class SmsQrPaymentTypeOrganizationSettingsMap : SubclassMap<SmsQrPaymentTypeOrganizationSettings>
	{
		public SmsQrPaymentTypeOrganizationSettingsMap()
		{
			DiscriminatorValue(nameof(PaymentType.SmsQR));
		}
	}
}
