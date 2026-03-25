using Vodovoz.Domain.Payments;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Application.Payments.OnlinePayments
{
	/// <inheritdoc/>
	public class DefaultYookassaOnlinePaymentRegisterColumns : IOnlinePaymentRegisterColumns
	{
		public int PaymentSumColumn => 1;
		public int DateAndTimeColumn => 4;
		public int PaymentNumberColumn => 6;
		public int? EmailColumn => 6;

		public static IOnlinePaymentRegisterColumns Create() => new DefaultYookassaOnlinePaymentRegisterColumns();
	}
}
