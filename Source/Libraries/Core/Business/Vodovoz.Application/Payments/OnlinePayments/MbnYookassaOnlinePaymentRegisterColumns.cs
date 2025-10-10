using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Application.Payments.OnlinePayments
{
	/// <inheritdoc/>
	public class MbnYookassaOnlinePaymentRegisterColumns : IOnlinePaymentRegisterColumns
	{
		public int PaymentSumColumn => 1;
		public int DateAndTimeColumn => 5;
		public int PaymentNumberColumn => 7;
		public int? EmailColumn => null;

		public static IOnlinePaymentRegisterColumns Create() => new MbnYookassaOnlinePaymentRegisterColumns();
	}
}
