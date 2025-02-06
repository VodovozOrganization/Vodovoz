using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Settings
{
	public class OnlinePaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public virtual IList<PaymentFrom> PaymentsFrom { get; set; } =  new List<PaymentFrom>();
		public override PaymentType PaymentType => PaymentType.PaidOnline;
	}
}
